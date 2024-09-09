<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CheckBoxView.aspx.cs" Inherits="AristaHRM.Areas.SPPD.Form.CheckBoxView" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:DetailsView ID="Details1" runat="server">
            <Fields>
                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:CheckBox ID="" runat="server" Checked="<%= Bind("") %>" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Fields>
        </asp:DetailsView>
        <dx:ASPxBinaryImage ID="" Eval="<%# Eval() %>" runat="server"></dx:ASPxBinaryImage>
        <input type="text" id="text1" name="txtTest" runat="server" />
        <asp:LinkButton ID="Link" runat="server" OnClientClick="getLabelText('<%# Eval("Order ID") %>', 'staticText')"></asp:LinkButton>
        <asp:LinkButton ID="lnkTest" runat="server" Text="Click" OnClientClick="javascript:test('<%# Eval("DocumentDescription")%>')"></asp:LinkButton>
        <input id="testing" runat="server" type="date" />

        <asp:LinkButton ID="Link2" runat="server" OnClientClick='<%# String.Format("getLabelText(\'{0}\', \'staticText\')", Eval("Order ID")) %>'></asp:LinkButton>
    </div>
    </form>
</body>
</html>
