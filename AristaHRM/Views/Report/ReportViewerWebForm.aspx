<%@ Page Language="C#" AutoEventWireup="True" Inherits="ReportViewerForMvc.ReportViewerWebForm" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="margin: 0px; padding: 0px;">
    <script runat="server">
        private void Page_Load(Object sender, EventArgs e)
        {
            Path.Value = (Session["ReportPath"] ?? String.Empty).ToString();

            ReportViewer1.ServerReport.ReportPath = Server.MapPath("~/Reports" + Path.Value);
        }
    </script>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
                <Scripts>
                    <asp:ScriptReference Assembly="ReportViewerForMvc" Name="ReportViewerForMvc.Scripts.PostMessage.js" />
                </Scripts>
            </asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" AsyncRendering="false"
                Font-Names="Verdana" Font-Size="8pt"
                WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%"
                Height="600px" runat="server">
            </rsweb:ReportViewer>
            <asp:HiddenField ID="Path" runat="server" />
            <asp:DropDownList ID="list" runat="server"></asp:DropDownList>
            <asp:GridView ID="Grid1" runat="server" OnDataBound="Grid1_DataBound" OnRowDataBound="Grid1_RowDataBound" OnSelectedIndexChanged="Grid1_SelectedIndexChanged">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="Button1" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <input type="text" id="text1" runat="server" />
            <asp:SqlDataSource ID="">
                <UpdateParameters>
                    <asp:SessionParameter DbType="Int32" Type="Decimal"
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>
    </form>
</body>
</html>
