using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmToolBox.Extensibility;

namespace DataverseUserSecurity.Helper
{
    public class HelperClass : PluginControlBase
    {
        /// <summary>
        /// Get Team Security Roles
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin control base</param>
        /// <param name="teamId">Team Id</param>
        /// <param name="rolesDict">Roles Dictionary</param>
        /// <returns>Returns Team Security Roles in a string separated by ;</returns>
        public static string GetTeamSecurityRolesBasedonTeamId(IOrganizationService service, PluginControlBase pluginControl, Guid teamId, Dictionary<Guid, string> rolesDict)
        {
            pluginControl.LogInfo("-----------------------------------------");

            LogInfoWithTimestamp(pluginControl, "Entered into GetTeamSecurityRoles Method");

            var teamSecurityRoles = string.Empty;

            // Query the teamroles intersect entity to get roleid for the team
            var roleQuery = new QueryExpression("teamroles")
            {
                ColumnSet = new ColumnSet("roleid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("teamid", ConditionOperator.Equal, teamId)
                        }
                }
            };

            LogInfoWithTimestamp(pluginControl, "Retrieve Team Security Roles");

            var entTeamRoles = service.RetrieveMultiple(roleQuery);

            LogInfoWithTimestamp(pluginControl, $"Total Team Security Roles found: {entTeamRoles.Entities.Count}");

            LogInfoWithTimestamp(pluginControl, "Processing Team Role(s)");

            foreach (var entity in entTeamRoles.Entities)
            {
                var roleId = entity.GetAttributeValue<Guid>("roleid");

                LogInfoWithTimestamp(pluginControl, $"Role ID: {roleId}");

                if (rolesDict != null && rolesDict.TryGetValue(roleId, out var roleName))
                {
                    teamSecurityRoles += roleName + "; ";

                    LogInfoWithTimestamp(pluginControl, $"Role Name: {roleName}");
                }
                else
                {
                    // Fallback to retrieve if not found in dictionary
                    var role = service.Retrieve("role", roleId, new ColumnSet("name"));
                    teamSecurityRoles += role.GetAttributeValue<string>("name") + ";";

                    LogInfoWithTimestamp(pluginControl, $"Role Name (from retrieve): {role.GetAttributeValue<string>("name")}");
                }
            }

            LogInfoWithTimestamp(pluginControl, teamSecurityRoles == string.Empty ? "No Team Security Roles found." : $"Total Team Security Roles processed: {entTeamRoles.Entities.Count} and Team Security Roles: {teamSecurityRoles}");

            LogInfoWithTimestamp(pluginControl, "Exiting from GetTeamSecurityRoles Method");
            pluginControl.LogInfo("-----------------------------------------");

            return teamSecurityRoles.TrimEnd(';', ' ');
        }

        /// <summary>
        /// Get Team Security Roles for multiple teams
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pluginControl"></param>
        /// <param name="teamIds"></param>
        /// <param name="securityRoles"></param>
        /// <returns></returns>
        public static string GetTeamSecurityRoles(IOrganizationService service, PluginControlBase pluginControl, List<Guid> teamIds, Dictionary<Guid, string> securityRoles)
        {
            string teamSecurityRoles = string.Empty;

            // Iterate through each team ID and accumulate their security roles
            foreach (var teamId in teamIds)
            {
                // Get roles for the current team
                var roles = GetTeamSecurityRolesBasedonTeamId(service, pluginControl, teamId, securityRoles);

                // Append roles to the cumulative string if not empty
                if (!string.IsNullOrEmpty(roles))
                {
                    teamSecurityRoles += roles + "; ";
                }
            }

            // Remove trailing semicolon and space
            teamSecurityRoles = teamSecurityRoles.TrimEnd(';', ' ');

            return teamSecurityRoles;
        }

        /// <summary>
        /// Get all the Security Roles in the system
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin control base</param>
        /// <returns>Returns all the Security Roles in dictionary</returns>
        public static Dictionary<Guid, string> GetAllSecurityRoles(IOrganizationService service, PluginControlBase pluginControl)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetAllSecurityRoles Method");

            var rolesDict = new Dictionary<Guid, string>();

            var query = new QueryExpression("role")
            {
                ColumnSet = new ColumnSet("roleid", "name")
            };

            LogInfoWithTimestamp(pluginControl, "Retrieving all Security Roles");

            var roles = service.RetrieveMultiple(query);

            foreach (var entity in roles.Entities)
            {
                var roleId = entity.GetAttributeValue<Guid>("roleid");
                var roleName = entity.GetAttributeValue<string>("name");

                if (!string.IsNullOrEmpty(roleName))
                {
                    rolesDict[roleId] = roleName;
                }
            }

            LogInfoWithTimestamp(pluginControl, $"Total Security Roles retrieved: {rolesDict.Count}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetAllSecurityRoles Method");
            pluginControl.LogInfo("-----------------------------------------");

            return rolesDict;
        }

        /// <summary>
        /// Get all the Team Ids for the provided User Id
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <param name="userId">User id</param>
        /// <param name="teamsDict">Team Dictionary</param>
        /// <returns>Returns all the user team Ids in list</returns>
        public static List<Guid> GetUserAllTeamIds(IOrganizationService service, PluginControlBase pluginControl, string userId, Dictionary<Guid, string> teamsDict)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetUserAllTeamIds Method");

            var teams = new List<Guid>();

            // Query the teammembership intersect entity for teams linked to the user
            var teamMembershipQuery = new QueryExpression("teammembership")
            {
                ColumnSet = new ColumnSet("teamid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                    }
                }
            };

            LogInfoWithTimestamp(pluginControl, "Retrieve User Team Memberships");
            var teamMemberships = service.RetrieveMultiple(teamMembershipQuery);
            Guid teamId;

            foreach (var membership in teamMemberships.Entities)
            {
                teamId = membership.GetAttributeValue<Guid>("teamid");

                LogInfoWithTimestamp(pluginControl, $"Team ID: {teamId.ToString()}");

                teams.Add(teamId);
            }

            LogInfoWithTimestamp(pluginControl, teams.Count == 0 ? "No Teams found." : $"Total Teams processed: {teams.Count}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetUserAllTeamIds Method");
            pluginControl.LogInfo("-----------------------------------------");

            return teams;
        }

        /// <summary>
        /// Get all the Team Names for the provided Team Ids
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <param name="teamIds">Team Ids in List</param>
        /// <param name="teamsDict">All teams Dictionary</param>
        /// <returns>Returns all the user team names in a string separated by ;</returns>
        public static string GetUserAllTeamNames(IOrganizationService service, PluginControlBase pluginControl, List<Guid> teamIds, Dictionary<Guid, string> teamsDict)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetUserAllTeamNames Method");

            var teamNames = string.Empty;

            LogInfoWithTimestamp(pluginControl, "Processing Team Name(s)");
            foreach (var teamId in teamIds)
            {
                if (teamsDict != null && teamsDict.TryGetValue(teamId, out var teamName))
                {
                    LogInfoWithTimestamp(pluginControl, $"Team ID: {teamId}, Team Name: {teamName}");
                    teamNames += teamName + "; ";
                }
            }

            LogInfoWithTimestamp(pluginControl, teamNames == string.Empty ? "No Team Names found." : $"Total Team Names processed: {teamIds.Count} and Team Names: {teamNames}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetUserAllTeamNames Method");
            pluginControl.LogInfo("-----------------------------------------");

            return teamNames.TrimEnd(';', ' ');
        }

        /// <summary>
        /// Get all the Teams in the system with paging
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <returns>Returns all the teams in Dictionary</returns>
        public static Dictionary<Guid, string> GetAllTeams(IOrganizationService service, PluginControlBase pluginControl)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetAllTeams Method");

            var teamsDictionary = new Dictionary<Guid, string>();

            var query = new QueryExpression("team")
            {
                ColumnSet = new ColumnSet("teamid", "name"),
                PageInfo = new PagingInfo
                {
                    Count = 5000,
                    PageNumber = 1,
                    PagingCookie = null
                }
            };

            EntityCollection results;

            do
            {
                LogInfoWithTimestamp(pluginControl, $"Retrieving Teams - Page Number: {query.PageInfo.PageNumber}");

                results = service.RetrieveMultiple(query);

                foreach (var team in results.Entities)
                {
                    Guid id = team.GetAttributeValue<Guid>("teamid");
                    LogInfoWithTimestamp(pluginControl, $"Team ID: {id}");

                    string name = team.GetAttributeValue<string>("name") ?? string.Empty;
                    LogInfoWithTimestamp(pluginControl, $"Team Name: {name}");

                    if (!teamsDictionary.ContainsKey(id))
                    {
                        teamsDictionary.Add(id, name);
                        LogInfoWithTimestamp(pluginControl, $"Team added to dictionary: ID = {id}, Name = {name}");
                    }
                }

                if (results.MoreRecords)
                {
                    LogInfoWithTimestamp(pluginControl, "More records found, preparing to retrieve next page.");

                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = results.PagingCookie;
                }

            } while (results.MoreRecords);

            LogInfoWithTimestamp(pluginControl, $"Total Teams retrieved: {teamsDictionary.Count}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetAllTeams Method");
            pluginControl.LogInfo("-----------------------------------------");

            return teamsDictionary;
        }

        /// <summary>
        /// Get User Security Roles
        /// </summary>
        /// <param name="service">Iorganization service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <param name="userId">User Guid</param>
        /// <param name="rolesDict">Roles Dictionary</param>
        /// <returns>Returns the Security roles in a string separated by ;</returns>
        public static string GetUserSecurityRoles(IOrganizationService service, PluginControlBase pluginControl, string userId, Dictionary<Guid, string> rolesDict)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetUserSecurityRoles Method");

            var userSecurityRoles = string.Empty;

            var roleQuery = new QueryExpression("systemuserroles")
            {
                ColumnSet = new ColumnSet("roleid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                    }
                }
            };

            LogInfoWithTimestamp(pluginControl, "Retrieve User Security Roles");
            var entUserRoles = service.RetrieveMultiple(roleQuery);

            foreach (var entity in entUserRoles.Entities)
            {
                var roleId = entity.GetAttributeValue<Guid>("roleid");

                LogInfoWithTimestamp(pluginControl, $"Role ID: {roleId}");

                if (rolesDict != null && rolesDict.TryGetValue(roleId, out var roleName))
                {
                    userSecurityRoles += roleName + "; ";

                    LogInfoWithTimestamp(pluginControl, $"Role Name: {roleName}");
                }
            }

            LogInfoWithTimestamp(pluginControl, userSecurityRoles == string.Empty ? "No User Security Roles found." : $"Total User Security Roles processed: {entUserRoles.Entities.Count} and User Security Roles: {userSecurityRoles}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetUserSecurityRoles Method");
            pluginControl.LogInfo("-----------------------------------------");

            return userSecurityRoles.TrimEnd(';', ' ');
        }

        /// <summary>
        /// Get all the Users in the system with paging
        /// </summary>
        /// <param name="service">Iorganization Service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <returns>Returns all the Users in the list</returns>
        public static List<Entity> GetAllUsers(IOrganizationService service, PluginControlBase pluginControl)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into GetAllUsers Method");

            var allUsers = new List<Entity>();
            int pageNumber = 1;
            string pagingCookie = null;
            bool moreRecords = false;

            do
            {
                LogInfoWithTimestamp(pluginControl, $"Retrieving Users - Page Number: {pageNumber}");
                var userQuery = new QueryExpression("systemuser")
                {
                    ColumnSet = new ColumnSet("systemuserid", "azureactivedirectoryobjectid", "fullname", "domainname", "businessunitid", "isdisabled", "accessmode", "applicationid", "createdon", "createdby", "modifiedon", "modifiedby", "internalemailaddress"),
                    PageInfo = new PagingInfo
                    {
                        Count = 5000,
                        PageNumber = pageNumber,
                        PagingCookie = pagingCookie
                    }
                };

                LogInfoWithTimestamp(pluginControl, "Retrieve Users");
                var users = service.RetrieveMultiple(userQuery);

                allUsers.AddRange(users.Entities);

                LogInfoWithTimestamp(pluginControl, $"Total Users retrieved so far: {allUsers.Count}");

                moreRecords = users.MoreRecords;

                if (moreRecords)
                {
                    LogInfoWithTimestamp(pluginControl, "More records found, preparing to retrieve next page.");
                    pageNumber++;
                    pagingCookie = users.PagingCookie;
                }
            } while (moreRecords);

            LogInfoWithTimestamp(pluginControl, $"Total Users retrieved: {allUsers.Count}");
            LogInfoWithTimestamp(pluginControl, "Exiting from GetAllUsers Method");
            pluginControl.LogInfo("-----------------------------------------");

            return allUsers;
        }

        /// <summary>
        /// Returns the count of rows in the provided DataTable as a string.
        /// </summary>
        /// <param name="dt">Data Table</param>
        /// <returns>returns the data table count in string</returns>
        public static string GetDataTableCount(DataTable dt)
        {
            return dt.Rows.Count.ToString();
        }

        /// <summary>
        /// Exports the provided DataTable to a CSV file at the specified file path.
        /// </summary>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <param name="dataTable">Data Table to be exported</param>
        /// <param name="filePath">File Path</param>
        public static void ExportDataTableToCsv(PluginControlBase pluginControl, DataTable dataTable, string filePath)
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into ExportDataTableToCsv Method");
            LogInfoWithTimestamp(pluginControl, "Starting export of DataTable to CSV");

            // Validate inputs
            if (dataTable == null || dataTable.Columns.Count == 0)
            {
                LogInfoWithTimestamp(pluginControl, "DataTable is null or empty. Exiting method.");
                throw new ArgumentException("DataTable is null or empty.");
            }

            // Validate file path
            if (string.IsNullOrWhiteSpace(filePath))
            {
                LogInfoWithTimestamp(pluginControl, "File path is null or empty. Exiting method.");
                throw new ArgumentException("File path is null or empty.");
            }

            var lines = new List<string>();

            LogInfoWithTimestamp(pluginControl, "Preparing CSV content");

            // Header
            var columnNames = dataTable.Columns.Cast<DataColumn>()
                .Select(column => "\"" + column.ColumnName.Replace("\"", "\"\"") + "\"");
            lines.Add(string.Join(",", columnNames));

            LogInfoWithTimestamp(pluginControl, "Writing data to CSV file");

            // Rows
            foreach (DataRow row in dataTable.Rows)
            {
                var fields = row.ItemArray.Select(field =>
                {
                    if (field == null)
                        return "\"\"";
                    var fieldString = field.ToString();
                    // Escape quotes
                    fieldString = fieldString.Replace("\"", "\"\"");
                    return $"\"{fieldString}\"";
                });
                lines.Add(string.Join(",", fields));
            }

            System.IO.File.WriteAllLines(filePath, lines, Encoding.UTF8);

            LogInfoWithTimestamp(pluginControl, $"Data successfully exported to {filePath}");
            pluginControl.LogInfo("-----------------------------------------");
        }

        /// <summary>
        /// Prompts the user with a SaveFileDialog to select a file path for saving a CSV file.
        /// </summary>
        /// <param name="pluginControl">Plugin contol base</param>
        /// <param name="defaultFileName">Default file Name</param>
        /// <returns>Returns the user selected file path with name</returns>
        public static string PromptUserForSaveFilePath(PluginControlBase pluginControl, string defaultFileName = "export.csv")
        {
            pluginControl.LogInfo("-----------------------------------------");
            LogInfoWithTimestamp(pluginControl, "Entered into PromptUserForSaveFilePath Method");
            LogInfoWithTimestamp(pluginControl, "Prompting user for save file path");

            using (var saveFileDialog = new SaveFileDialog())
            {
                LogInfoWithTimestamp(pluginControl, "Configuring SaveFileDialog");

                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = defaultFileName;
                saveFileDialog.Title = "Select location to save CSV file";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LogInfoWithTimestamp(pluginControl, $"User selected file path: {saveFileDialog.FileName}");
                    pluginControl.LogInfo("-----------------------------------------");

                    return saveFileDialog.FileName;
                }
                return null;
            }
        }

        /// <summary>
        /// Log information with a timestamp prefix.
        /// </summary>
        /// <param name="pluginControl">Plugin control base</param>
        /// <param name="message">Message to be displayed</param>
        public static void LogInfoWithTimestamp(PluginControlBase pluginControl, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            pluginControl.LogInfo($"[{timestamp}] {message}");
        }
    }
}
