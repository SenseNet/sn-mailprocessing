************************************************************************************
                                 sensenet platform
                                   MailProcessing
************************************************************************************

To finalize the installation and get started with sensenet MailProcessing, please follow these steps:

1. Build your solution, make sure that there are no build errors.

2. Install sensenet MailProcessing using SnAdmin

   - open a command line and go to the \Admin\bin folder
   - execute the install-mailprocessing command with the SnAdmin tool
      - optional parameters:
         - overwriteemailworkflow: if you execute the package with a 'false' value for this parameter,
								   it will set the default mail processor workflow on content lists
								   only if the 'Incoming email workflow' field is empty and will not
								   overwrite existing references. Default is true.

   .\snadmin install-mailprocessing

You are good to go! Hit F5 and start integrating your email server!
For more information and support, please visit https://sensenet.com