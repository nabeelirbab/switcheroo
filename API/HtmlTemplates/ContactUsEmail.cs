namespace API.HtmlTemplates
{
    public class ContactUsEmail
    {
        public ContactUsEmail(string serverBasePath, string senderEmail, string title, string description, string name)
        {
            this.serverBasePath = serverBasePath;
            this.title = title;
            this.description = description;
            this.senderEmail = senderEmail;
            this.name = name;
        }

        private readonly string serverBasePath;
        private readonly string title;
        private readonly string description;
        private readonly string senderEmail;
        private readonly string name;

        public string GetHtmlString()
        {
            return $@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta http-equiv=""Content-Type"" content=""text/html charset=UTF-8"" />
                    </head>
                    <body
                        style=""
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen,
                            Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
                        padding: 0;
                        margin: 0;
                        background: white;
                        ""
                    >
                        <table
                        cellpadding=""0""
                        cellspacing=""0""
                        style=""
                            border-radius: 5px;
                            -moz-border-radius: 5px;
                            border: 1px solid #e0e0e0;
                            margin: 10px auto;
                            max-width: 700px;
                        ""
                        >
                        <tr>
                            <td>
                            <table
                                style=""
                                border-radius: 5px;
                                -moz-border-radius: 5px;
                                padding: 0;
                                border-collapse: collapse;
                                overflow: hidden;
                                background-color: white;
                                max-width: 700px;
                                ""
                            >
                                <thead>
                                <tr
                                    style=""color: white; height: 55px; background-color: #30b69c;""
                                >
                                    <th style=""text-align: center;"">
                                    <img
                                        src=""{serverBasePath}/Assets/SwitcherooLogoWhite.png""
                                        width=""120""
                                        height=""30""
                                        style=""height: auto; font-size: 22px; color: white;""
                                        alt=""Switcheroo""
                                    />
                                    </th>
                                </tr>
                                </thead>

                                <tbody>
                                <tr style=""color: #404040; font-size: 16px; font-weight: bold;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    {title}
                                    </td>
                                </tr>
                                <tr style=""color: #404040; font-size: 14px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    {name}
                                    </td>
                                </tr>
                                <tr style=""color: #404040; font-size: 14px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    {description}
                                    </td>
                                </tr>
                                <tr style=""color: #606060; font-size: 12px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    Sender: {senderEmail}
                                    </td>
                                </tr>
                                </tbody>
                            </table>
                            </td>
                        </tr>
                        </table>
                    </body>
                </html>
            ";
        }
    }
}
