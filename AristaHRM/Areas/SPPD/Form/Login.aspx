﻿<%@ Page Title="Login" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SPD.Form.Login" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Testing SPPD</title>
    
    <style>
        header {
            font-family: 'Monotype Corsiva';
            font-size: 5vw;
            width: auto;
            height: 100px;
            background-color: #00A1F1;
            padding-top: 35px;
            text-align:center;
        }

        section {
            padding-top: 50px;
            padding-bottom: 20px;
            background-color: white;
            margin-left: auto;
            margin-right: auto;
            width: auto;
            height: 150px;
            align-items: center;
            font-size: 15px;
            font-family: Arial, serif;
        }

        body {
            padding-top: 50px;
            width: 100%;
            height: auto;
            background-color: #f6f6f6;

        }

        footer {
            align-items: center;
            margin-left: auto;
            margin-right: auto;
            width: auto;
            height: 50px;
            padding-top:20px;
            color:white;
            text-align:center;
            background-color: #00A1F1;
        }

        #wrapper {
            margin: 0 auto;
            max-width: 800px;
            box-shadow: 0 0 6px #00A1F1;
        }

        .auto-style2 {
            height: 50px;
        }

        .auto-style3 {
            height: 24px;
        }

        .auto-style5 {
            height: 33px;
        }
    </style>
</head>
<body>
    <div id="wrapper">
        <header style="text-align: center; color: white; padding-left:50px;">
            <table>
                <tr>
                    <td>
                      <dx:ASPxImage ID="img" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/as.jpg" Width="400px" Height="80px"></dx:ASPxImage>
                    </td>
                </tr>
            </table>
            

        </header>

        <section>
            <form id="form1" runat="server">

                <div>
                    <table style="margin-left: auto; margin-right: auto;">
                        <tr style="font-size: 16px; font-weight: bold;">
                            <td class="auto-style5">
                                <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="NIK" Font-Bold="True"  ForeColor="#0033cc" ></dx:ASPxLabel>
                            </td>
                            <td class="auto-style5">
                                <dx:ASPxLabel ID="ASPxLabel2" runat="server" Text=":"  ForeColor="#0033cc"></dx:ASPxLabel>
                            </td>
                            <td class="auto-style5">
                                <dx:ASPxTextBox ID="username" runat="server" Width="170px" AutoPostBack="true" OnTextChanged="username_TextChanged"></dx:ASPxTextBox>
                            </td>
                            
                        </tr>

                        <tr>
                            <td></td>
                            <td></td>
                            <td class="auto-style5">
                                <dx:ASPxTextBox ID="txtnama" runat="server" Width="170px" Enabled="false" BackColor="#e6e6e6" ForeColor="Black"></dx:ASPxTextBox>
                            </td>
                            <td class="auto-style5">
                                <dx:ASPxTextBox ID="txtprivilege" runat="server" Width="80px" Enabled="false" BackColor="#e6e6e6" ForeColor="Black" Visible="false"></dx:ASPxTextBox>
                            </td>
                             <td class="auto-style5">
                                <dx:ASPxTextBox ID="txtjabatan" runat="server" Width="170px" Enabled="false" BackColor="#e6e6e6" ForeColor="Black" Visible="false"></dx:ASPxTextBox>
                            </td>

                        </tr>

                        <tr style="font-size: 16px; font-weight: bold;">
                            <td class="auto-style5">
                                <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text="Password" Font-Bold="True" ForeColor="#0033cc"></dx:ASPxLabel>
                            </td>
                            <td class="auto-style5">
                                <dx:ASPxLabel ID="ASPxLabel4" runat="server" Text=":"  ForeColor="#0033cc"></dx:ASPxLabel>
                            </td>
                            <td class="auto-style5">
                                <dx:ASPxTextBox ID="password" runat="server" Password="True" Width="170px"></dx:ASPxTextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3" class="auto-style2" style="text-align: center;">
                                <dx:ASPxButton ID="Submit" runat="server" Text="Submit" OnClick="Submit_Click"></dx:ASPxButton>
                            </td>
                        </tr>
                        <tr style="font-size: 15px; font-weight: bold; text-align: center;">
                            <td colspan="3" class="auto-style3">
                                <dx:ASPxLabel ID="label" runat="server" Text="Wrong Password or Username" ForeColor="Red" Visible="False" Font-Bold="True"></dx:ASPxLabel>
                            </td>
                        </tr>
                    </table>
                </div>
            </form>
        </section>

        <footer style="font-family:Calibri">
            &copy; <%: DateTime.Now.Year %>  - Arista Group . All Right Reserved
                                               
        </footer>
    </div>
</body>
</html>
