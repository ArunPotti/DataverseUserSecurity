using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataverseUserSecurity.Helper;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;

namespace DataverseUserSecurity
{
    public partial class MyPluginControl : PluginControlBase, IStatusBarMessenger, IGitHubPlugin, IPayPalPlugin, IHelpPlugin
    {
        #region XRMToolBox Interfaces

        #region IGitHubPlugin implementation
        public string RepositoryName => "DataverseUserSecurity";
        public string UserName => "ArunPotti";
        #endregion IGitHubPlugin implementation

        #region IPayPalPlugin implementation
        public string DonationDescription => "Buy me a coffee !!!";
        public string EmailAccount => "arun.p965@gmail.com";
        #endregion IPayPalPlugin implementation

        #region IHelpPlugin implementation
        public string HelpUrl => "https://github.com/ArunPotti/DataverseUserSecurity";
        #endregion

        #region IStatusBarMessenger implementation
        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;
        #endregion IStatusBarMessenger implementation

        #endregion XRMToolBox Interfaces

        private Settings mySettings;

        private DataTable dtUserData = null, dtExportData = null;

        private const string totalNumberOfUsersText = "Total number of users retrieved: ";

        private PluginControlBase pluginControl = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyPluginControl"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the control by initializing its components and assigning the
        /// current instance to a static reference for plugin control management.</remarks>
        public MyPluginControl()
        {
            InitializeComponent();

            pluginControl = this;
        }

        /// <summary>
        /// Handles the load event for the plugin control.
        /// </summary>
        /// <remarks>This method initializes the plugin by displaying a notification and loading the
        /// plugin's settings. If no settings are found, a new settings file is created and a warning is
        /// logged.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            // ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            // Auto adjust DataGrid size and position of label when the control is resized
            DataGridSizeAutoAdjust();
        }

        /// <summary>
        /// Adjust the DataGridView size when the control is resized
        /// </summary>
        private void DataGridSizeAutoAdjust()
        {
            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // Adjust the DataGridView width 
            dgUsersData.Width = workingRectangle.Width - 20; // Adjust width with some padding

            // Adjust the DataGridView height with some padding
            dgUsersData.Height = workingRectangle.Height - toolStripMenu.Height - lblRetrievedUsers.Height - 150;
        }

        /// <summary>
        /// Handles the click event for the "Close" toolbar button.
        /// </summary>
        /// <remarks>This method saves the current settings before closing the tool.</remarks>
        /// <param name="sender">The source of the event, typically the toolbar button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsbClose_Click(object sender, EventArgs e)
        {
            LogInfo("Call tsbClose_Click Method");

            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
            CloseTool();
        }

        /// <summary>
        /// Handles the event triggered when the tool is closed.
        /// </summary>
        /// <remarks>This method ensures that the current settings are saved before the tool is
        /// closed.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            LogInfo("Call MyPluginControl_OnCloseTool Method");

            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// Updates the connection details and applies the specified action to the new service.
        /// </summary>
        /// <remarks>If <paramref name="detail"/> is provided, the method updates the last used
        /// organization web application URL in the settings and logs the change. Ensure that <paramref name="detail"/>
        /// is not null to avoid unexpected behavior.</remarks>
        /// <param name="newService">The new <see cref="IOrganizationService"/> instance to use for the connection.</param>
        /// <param name="detail">The details of the new connection, including the web application URL. Cannot be null.</param>
        /// <param name="actionName">The name of the action to perform during the connection update. This can be null or empty if no specific
        /// action is required.</param>
        /// <param name="parameter">An optional parameter associated with the action. This can be null if the action does not require additional
        /// data.</param>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            LogInfo("Call UpdateConnection Method");

            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        /// <summary>
        /// Handles the click event for the "Load Data" toolbar button.
        /// </summary>
        /// <param name="sender">The source of the event, typically the toolbar button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsbLoadData_Click(object sender, EventArgs e)
        {
            LogInfo("Call tsbLoadData_Click Method");

            ExecuteMethod(LoadData, pluginControl);
        }

        /// <summary>
        /// Loads users, teams, and security roles, team security roles data asynchronously and binds the results to the user interface.
        /// </summary>
        /// <remarks>This method retrieves system users, security roles, teams, and their relationships
        /// from the service. The data is processed asynchronously and displayed in a data grid. If an error occurs
        /// during the operation, an error message is displayed to the user. The method also updates the UI with the
        /// total number of users retrieved.</remarks>
        /// <param name="pluginControl">The plugin control instance used to retrieve data from the service.</param>
        private void LoadData(PluginControlBase pluginControl)
        {
            LogInfo("-------------------------------");
            LogInfo("Call LoadData Method");
            LogInfo("Retrieving All System Users, Security Roles, Teams, Team Security Roles. Please wait...");

            DataTable userDt = new DataTable();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Retrieving System Users, Security Roles, Teams, Team Security Roles. Please wait...",
                AsyncArgument = null,
                Work = (worker, args) =>
                {
                    RaiseStatusBarMessage("Started retrieving the Users data.");
                    LogInfo("Started retrieving the Users data.");

                    // Get all the System Users
                    var allUsers = HelperClass.GetAllUsers(Service, pluginControl);

                    RaiseStatusBarMessage("Got all the Users data.");
                    LogInfo("Got all the Users data.");

                    RaiseStatusBarMessage("Started retreiving all the Teams data.");
                    LogInfo("Started retreiving all the Teams data.");

                    // Get all the Teams
                    var allTeams = HelperClass.GetAllTeams(Service, pluginControl);

                    RaiseStatusBarMessage("Got all the Teams data.");
                    LogInfo("Got all the Teams data.");

                    RaiseStatusBarMessage("Started retreiving all the Security Roles data.");
                    LogInfo("Started retreiving all the Security Roles data.");

                    // Get all the Security Roles
                    var allSecurityRoles = HelperClass.GetAllSecurityRoles(Service, pluginControl);

                    RaiseStatusBarMessage("Got all the Security Roles data.");
                    LogInfo("Got all the Security Roles data.");

                    RaiseStatusBarMessage("Started retrieving the User Roles, Teams and Teams Security Roles");
                    LogInfo("Started retrieving the User Roles, Teams and Teams Security Roles");

                    // Get the User Data Table
                    args.Result = GetUserDataTable(Service, pluginControl, allUsers, allTeams, allSecurityRoles);

                    RaiseStatusBarMessage("Retrieved all the User details, Security Roles, Teams and Teams Security Roles");
                    LogInfo("Retrieved all the User details, Security Roles, Teams and Teams Security Roles");
                },
                PostWorkCallBack = (args) =>
                {
                    // Check if there was an error during the work
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.Message, "Error...!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Check if the result is null
                    if (args.Result == null)
                    {
                        MessageBox.Show("No data found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Check if the result is of type List<Entity>
                    if (!(args.Result is DataTable))
                    {
                        MessageBox.Show("Unexpected result type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Cast the result to DataTable
                    dtUserData = args.Result as DataTable;

                    // Bind the DataGridView to the DataTable
                    dgUsersData.DataSource = dtUserData;

                    // Create a copy of the DataTable for export purposes
                    dtExportData = dtUserData.Copy();

                    // Update the label with the total number of users retrieved
                    lblRetrievedUsers.Text = string.Concat(totalNumberOfUsersText, HelperClass.GetDataTableCount(dtUserData));
                    LogInfo("Total number of users retrieved: {0}", HelperClass.GetDataTableCount(dtUserData));
                    ShowInfoNotification("All user details and security related information retrieved successfully", null);
                }
            });

            LogInfo("-------------------------------");
        }

        /// <summary>
        /// Exports the currently displayed data to a CSV file.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Export toolbar button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsbExport_Click(object sender, EventArgs e)
        {
            LogInfo("-------------------------------");
            LogInfo("Call tsbExport_Click Method");

            var filePath = string.Empty;

            // Check if there is data to export
            if (dtExportData != null)
            {
                // If dtExportData is not null, then there is data to export; Otherwise there is no data to export
                if (dtExportData.Rows.Count > 0)
                {
                    LogInfo("There are {0} records to export", dtExportData.Rows.Count);

                    // Prompt the user for a file path to save the CSV
                    filePath = HelperClass.PromptUserForSaveFilePath(pluginControl);

                    // If the user provided a file path, export the data to CSV
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        LogInfo("Exporting data to CSV at: {0}", filePath);

                        // Export the DataTable to CSV
                        HelperClass.ExportDataTableToCsv(pluginControl, dtExportData, filePath);

                        ShowInfoNotification("Data exported successfully to " + filePath, null);

                        MessageBox.Show("Data exported successfully to " + filePath, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LogInfo("Data exported successfully to: {0}", filePath);
                    }
                }
                else
                {
                    MessageBox.Show("No data to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogInfo("No data to export as there are 0 records");
                }
            }
            else
            {
                MessageBox.Show("No data to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LogInfo("No data to export as there are 0 records");
            }

            LogInfo("-------------------------------");
        }

        /// <summary>
        /// Search text changed event handler for the search textbox.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Text Search button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsbTxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            LogInfo("Call tsbTxtSearch_KeyUp Method");

            // If there is no data, update the label accordingly and exit
            if (dtUserData == null || dtUserData.Rows.Count == 0)
            {
                lblRetrievedUsers.Text = totalNumberOfUsersText + "0";
                return;
            }

            // Cast the sender to ToolStripTextBox
            var textBox = sender as ToolStripTextBox;

            // If textBox is null, exit the method to avoid null reference exceptions
            if (textBox == null) return;

            LogInfo("Search Text: {0}", textBox.Text);

            string searchText = textBox.Text?.Trim();

            // If the search text is empty, reset the DataGridView to show all data
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LogInfo("Search text is empty, resetting to show all data.");

                // Optionally clear results or reset to all data
                dgUsersData.DataSource = dtUserData;

                // Create a copy of the DataTable for export purposes
                dtExportData = dtUserData.Copy();

                // Update the label with the total number of users retrieved
                lblRetrievedUsers.Text = totalNumberOfUsersText + dtUserData.Rows.Count;

                return;
            }

            // Perform in-memory search on all columns
            string filter = string.Join(" OR ", dtUserData.Columns
                .Cast<DataColumn>()
                .Select(col => $"CONVERT([{col.ColumnName}], System.String) LIKE '%{searchText.Replace("'", "''")}%'"));

            LogInfo("Filter applied: {0}", filter);

            try
            {
                DataRow[] filteredRows = dtUserData.Select(filter);

                // If there are matching records, display them; otherwise, show an empty table
                if (filteredRows.Length > 0)
                {
                    LogInfo("Number of matching records found: {0}", filteredRows.Length);

                    DataTable filteredTable = dtUserData.Clone();

                    // Import the filtered rows into the new DataTable
                    foreach (var row in filteredRows)
                        filteredTable.ImportRow(row);

                    // Bind the filtered DataTable to the DataGridView
                    dgUsersData.DataSource = filteredTable;

                    // Create a copy of the filtered DataTable for export purposes
                    dtExportData = filteredTable.Copy();

                    // Update the label with the total number of users retrieved
                    lblRetrievedUsers.Text = string.Concat(totalNumberOfUsersText, HelperClass.GetDataTableCount(filteredTable));
                }
                else
                {
                    LogInfo("No matching records found.");

                    // No results, show empty table
                    dgUsersData.DataSource = dtUserData.Clone();

                    // Create a copy of the empty DataTable for export purposes
                    dtExportData = dtUserData.Copy();

                    // Update the label with the total number of users retrieved
                    lblRetrievedUsers.Text = string.Concat(totalNumberOfUsersText, HelperClass.GetDataTableCount(dtUserData));
                }
            }
            catch
            {
                LogError("Error occurred while applying filter: {0}", filter);

                // Fallback: show all data if filter fails
                dgUsersData.DataSource = dtUserData;

                // Update the label with the total number of users retrieved
                lblRetrievedUsers.Text = string.Concat(totalNumberOfUsersText, HelperClass.GetDataTableCount(dtUserData));
            }
        }

        /// <summary>
        /// Logs a message to the status bar.
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        private void RaiseStatusBarMessage(string message)
        {
            // Invoke the event to send the message to the status bar
            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(message));
        }

        /// <summary>
        /// Retrieves user data and populates a DataTable with user details, including security roles, team and team security roles information.
        /// </summary>
        /// <param name="service">IOrganization Service</param>
        /// <param name="pluginControl">Plugin Control Base</param>
        /// <param name="userDataListEntity">All System users data</param>
        /// <param name="allTeams">All Teams data</param>
        /// <param name="allSecurityRoles">All Security roles data</param>
        /// <returns>Returns User data table set</returns>
        private DataTable GetUserDataTable(IOrganizationService service, PluginControlBase pluginControl, List<Entity> userDataListEntity, Dictionary<Guid, string> allTeams, Dictionary<Guid, string> allSecurityRoles)
        {
            // Initialize variables to hold user details
            var userSecurityRoles = "";
            var userTeams = "";
            var userTeamsSecurityRoles = "";
            var userId = "";
            var userAzureAdObjectId = "";
            var userFullName = "";
            var userBusinessUnitId = "";
            var userBusinessUnitName = "";
            var isDisabled = false;
            var userStatus = "";
            var userAccessMode = "";
            var userApplicationId = "";
            var userDomainName = "";
            var userCreatedOn = "";
            var userCreatedBy = "";
            var userModifiedOn = "";
            var userModifiedBy = "";
            var userPrimaryEmail = "";
            EntityReference userBusinessUnit = null;
            List<Guid> allUserTeamIds = null;

            pluginControl.LogInfo("-----------------------------------------");
            HelperClass.LogInfoWithTimestamp(pluginControl, "Entered into GetUserDataTable Method");

            int userCount = 0,
                userDataTableCount = 0;

            DataTable userDataTable = new DataTable();

            HelperClass.LogInfoWithTimestamp(pluginControl, "Processing User(s)");

            if (userDataListEntity != null)
            {
                // Get the Users List Total count
                userDataTableCount = userDataListEntity.Count;

                HelperClass.LogInfoWithTimestamp(pluginControl, $"Total Users to be processed: {userDataTableCount}");

                // Define the columns for the DataTable
                userDataTable.Columns.Add("User ID", typeof(Guid));
                userDataTable.Columns.Add("Application ID", typeof(string));
                userDataTable.Columns.Add("Azure AD Object Id", typeof(string));
                userDataTable.Columns.Add("Full Name", typeof(string));
                userDataTable.Columns.Add("Domain Name", typeof(string));
                userDataTable.Columns.Add("Business Unit ID", typeof(string));
                userDataTable.Columns.Add("Business Unit Name", typeof(string));
                userDataTable.Columns.Add("Primary Email", typeof(string));
                userDataTable.Columns.Add("User Status", typeof(string));
                userDataTable.Columns.Add("Access Mode", typeof(string));
                userDataTable.Columns.Add("User Security Roles", typeof(string));
                userDataTable.Columns.Add("User Teams", typeof(string));
                userDataTable.Columns.Add("Teams Security Roles", typeof(string));
                userDataTable.Columns.Add("Created on", typeof(DateTime));
                userDataTable.Columns.Add("Created by", typeof(string));
                userDataTable.Columns.Add("Modified on", typeof(DateTime));
                userDataTable.Columns.Add("Modified by", typeof(string));

                HelperClass.LogInfoWithTimestamp(pluginControl, "Populating User Data Table");

                pluginControl.LogInfo("*-----------------------------------------*");

                // Iterate through each user entity and populate the DataTable
                foreach (var userEntity in userDataListEntity)
                {
                    pluginControl.LogInfo("-----------------------------------------");

                    userCount = userCount + 1;

                    HelperClass.LogInfoWithTimestamp(pluginControl, $"Retrieving user {userCount} of {userDataTableCount}");

                    RaiseStatusBarMessage($"Retrieving user {userCount} of {userDataTableCount}");

                    // Get the User Id
                    userId = userEntity.GetAttributeValue<Guid?>("systemuserid")?.ToString() ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User ID: {userId}");

                    // Get the Azure AD Object Id
                    userAzureAdObjectId = userEntity.GetAttributeValue<Guid?>("azureactivedirectoryobjectid") != null && userEntity.GetAttributeValue<Guid>("azureactivedirectoryobjectid") != Guid.Empty
                        ? userEntity.GetAttributeValue<Guid>("azureactivedirectoryobjectid").ToString()
                        : string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Azure AD Object ID: {userAzureAdObjectId}");

                    // Get the User Full Name
                    userFullName = userEntity.GetAttributeValue<string>("fullname") ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Full Name: {userFullName}");

                    // Get the User Business Unit
                    userBusinessUnit = null;
                    userBusinessUnit = userEntity.GetAttributeValue<EntityReference>("businessunitid");

                    // Get the User Business Unit Id
                    userBusinessUnitId = userBusinessUnit?.Id.ToString() ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Business Unit ID: {userBusinessUnitId}");

                    // Get the Business Unit Name
                    userBusinessUnitName = userBusinessUnit?.Name ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Business Unit Name: {userBusinessUnitName}");

                    // Get the User status 
                    isDisabled = userEntity.Contains("isdisabled") && userEntity.GetAttributeValue<bool?>("isdisabled") == true;
                    userStatus = isDisabled ? "Disabled" : "Enabled";
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Status: {userStatus}");

                    // Get the User Access Mode
                    userAccessMode = userEntity.FormattedValues != null && userEntity.FormattedValues.ContainsKey("accessmode")
                        ? userEntity.FormattedValues["accessmode"] ?? string.Empty
                        : string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Access Mode: {userAccessMode}");

                    // Check for Application Id
                    userApplicationId = userEntity.GetAttributeValue<Guid?>("applicationid") != null && userEntity.GetAttributeValue<Guid>("applicationid") != Guid.Empty
                        ? userEntity.GetAttributeValue<Guid>("applicationid").ToString()
                        : string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Application ID: {userApplicationId}");

                    // Get the User Domain Name
                    userDomainName = userEntity.GetAttributeValue<string>("domainname") ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Domain Name: {userDomainName}");

                    // Get the Created On date
                    userCreatedOn = userEntity.GetAttributeValue<DateTime?>("createdon")?.ToString() ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Created On: {userCreatedOn}");

                    // Get the Created By user
                    var createdByRef = userEntity.GetAttributeValue<EntityReference>("createdby");
                    userCreatedBy = createdByRef?.Name ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Created By: {userCreatedBy}");

                    // Get the Modified On date
                    userModifiedOn = userEntity.GetAttributeValue<DateTime?>("modifiedon")?.ToString() ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Modified On: {userModifiedOn}");

                    // Get the Modified By user
                    var modifiedByRef = userEntity.GetAttributeValue<EntityReference>("modifiedby");
                    userModifiedBy = modifiedByRef?.Name ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Modified By: {userModifiedBy}");

                    // Get the Primary Email
                    userPrimaryEmail = userEntity.GetAttributeValue<string>("internalemailaddress") ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Primary Email: {userPrimaryEmail}");

                    // Get the User Security Roles
                    userSecurityRoles = string.Empty;
                    userSecurityRoles = HelperClass.GetUserSecurityRoles(service, pluginControl, userId, allSecurityRoles) ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Security Roles: {userSecurityRoles}");

                    // Get all the User Teams Ids
                    allUserTeamIds = null;
                    allUserTeamIds = HelperClass.GetUserAllTeamIds(service, pluginControl, userId, allTeams);

                    // Get the User Teams
                    userTeams = string.Empty;
                    userTeams = HelperClass.GetUserAllTeamNames(service, pluginControl, allUserTeamIds, allTeams) ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Teams: {userTeams}");

                    // Get the User Teams Security Roles
                    userTeamsSecurityRoles = string.Empty;
                    userTeamsSecurityRoles = HelperClass.GetTeamSecurityRoles(service, pluginControl, allUserTeamIds, allSecurityRoles) ?? string.Empty;
                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User Teams Security Roles: {userTeamsSecurityRoles}");

                    // Add the user data to the DataTable
                    userDataTable.Rows.Add(
                        userId,
                        userApplicationId,
                        userAzureAdObjectId,
                        userFullName,
                        userDomainName,
                        userBusinessUnitId,
                        userBusinessUnitName,
                        userPrimaryEmail,
                        userStatus,
                        userAccessMode,
                        userSecurityRoles,
                        userTeams,
                        userTeamsSecurityRoles,
                        userCreatedOn,
                        userCreatedBy,
                        userModifiedOn,
                        userModifiedBy
                    );

                    HelperClass.LogInfoWithTimestamp(pluginControl, $"User {userFullName} added to Data Table");
                    pluginControl.LogInfo("-----------------------------------------");
                }
            }

            HelperClass.LogInfoWithTimestamp(pluginControl, $"Total Users processed and added to Data Table: {userCount}");
            HelperClass.LogInfoWithTimestamp(pluginControl, "Exiting from GetUserDataTable Method");
            pluginControl.LogInfo("-----------------------------------------");

            // Return the populated DataTable
            return userDataTable;
        }
    }
}