namespace API.HtmlTemplates
{
    public class SignUpConfirmationEmail
    {
        public SignUpConfirmationEmail(string serverBasePath, string code, string email, string name)
        {
            this.serverBasePath = serverBasePath;
            this.code = code;
            this.email = email;
            this.name = name;
        }

        private readonly string serverBasePath;
        private readonly string code;
        private readonly string email;
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
                                    Please verify your email address
                                    </td>
                                </tr>
                                <tr style=""color: #404040; font-size: 14px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    There's one quick step you need to complete before creating
                                    your Switcheroo account. Let's make sure this is the right
                                    email address by allowing you to enter the verification code
                                    below.
                                    </td>
                                </tr>
                                <tr
                                    style=""
                                    color: #26927d;
                                    font-size: 24px;
                                    line-height: 28px;
                                    font-weight: bold;
                                    ""
                                >
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    {code}
                                    </td>
                                </tr>
                                <tr style=""color: #606060; font-size: 12px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    This verification code will expire after two hours.
                                    </td>
                                </tr>
                                <tr style=""color: #404040; font-size: 14px; line-height: 28px;"">
                                    <td style=""padding: 20px; padding-bottom: 0;"">
                                    Regards,
                                    </td>
                                </tr>
                                <tr style=""color: #404040; font-size: 14px; font-weight: bold;"">
                                    <td style=""padding: 20px; padding-top: 0;"">
                                    The Switcheroo Team
                                    </td>
                                </tr>
                                <tr
                                    style=""
                                    border-top: 1px solid #e0e0e0;
                                    color: #808080;
                                    font-size: 12px;
                                    ""
                                >
                                    <td style=""padding: 20px;"">
                                    This message was sent to {email} and intended for {name}
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
