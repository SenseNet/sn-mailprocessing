# Mail processing component for the sensenet platform
Legacy package for handling content list mail server integration in sensenet. It is able to connect a sensenet Content List or Document Library to an **Exchange** or **POP3** mailbox, pull emails sent to the configured mailbox and save emails and attachments to the library.

This package has the following legacy dependencies:

- [Microsoft.Exchange.WebServices](https://www.nuget.org/packages/Microsoft.Exchange.WebServices/)
- [OpenPop.NET](https://www.nuget.org/packages/OpenPop.NET/)

Both packages are built on an older version of .Net Framework so cannot be used in a _.Net Core_ environment.

Also, the Exchange package is built on a feature that will be deprecated: https://developer.microsoft.com/en-us/graph/blogs/upcoming-changes-to-exchange-web-services-ews-api-for-office-365/

# sensenet as a service (SNaaS) - use sensenet from the cloud

For a monthly subscription fee, we store all your content and data, relieving you of all maintenance-related tasks and installation, ensuring easy onboarding, easy updates, and patches.

https://www.sensenet.com/pricing
