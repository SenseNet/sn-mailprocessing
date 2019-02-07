# Mail processing component for the sensenet platform
Legacy package for handling content list mail server integration in sensenet. It is able to connect a sensenet Content List or Document Library to an **Exchange** or **POP3** mailbox, pull emails sent to the configured mailbox and save emails and attachments to the library.

For details please take a look at this article: http://wiki.sensenet.com/Sending_Emails_to_Content_Lists

This package has the following legacy dependencies:

- [Microsoft.Exchange.WebServices](https://www.nuget.org/packages/Microsoft.Exchange.WebServices/)
- [OpenPop.NET](https://www.nuget.org/packages/OpenPop.NET/)

Both packages are built on an older version of .Net Framework so cannot be used in a _.Net Core_ environment.

Also, the Exchange package is built on a feature that will be deprecated: https://developer.microsoft.com/en-us/graph/blogs/upcoming-changes-to-exchange-web-services-ews-api-for-office-365/
