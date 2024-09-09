<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Areas/SPPD/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="SPD.Form.Home" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxMenu" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxSplitter" TagPrefix="dx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="Selamat datang" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True">
        <PanelCollection>
            <dx:PanelContent>
                <div style="text-align:center;">
                    <table>
                          <tr>
                              <dx:ASPxImage ID="img" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/SPDlogo.jpg" Width="930px" Height="200px"></dx:ASPxImage>
                          </tr>
                   </table>
                    
                </div>
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>


    <dx:ASPxRoundPanel ID="ASPxRoundPanel1" runat="server" Width="100%" HeaderText="User Navigation" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True">
        <PanelCollection>
            <dx:PanelContent>
                <div style="text-align:center; Color:GrayText; font-size:15px;">
                    <table style="text-align:center; padding-left:10px;">
                        <tr>
                            <td style="width:330px; height:60px;">SPD Form 1 (FORM PENGAJUAN)</td>
                            <td style="width:330px;">SPD Form 2 (PERTANGGUNG-JAWABAN)</td>
                            <td style="width:300px;">ABOUT SYSTEM</td>
                            <td style="width:300px;">LOG-OUT</td>
                        </tr>
                         <tr>
                             <td><a href="spd1.aspx"><dx:ASPxImage ID="ASPxImage1" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/datas1.png" Width="180px" Height="170px" Title="SPD Form 1 (FORM PENGAJUAN)" Cursor="pointer"></dx:ASPxImage></a></td>
                             <td><a href="spd2.aspx"><dx:ASPxImage ID="ASPxImage2" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/datas2.png" Width="180px" Height="170px" Title="SPD Form 2 (PERTANGGUNG-JAWABAN)" Cursor="pointer"></dx:ASPxImage></a></td>
                             <td><a href="Help.aspx"><dx:ASPxImage ID="ASPxImage3" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/help.png" Width="160px" Height="160px" Title="Panduan Umum" Cursor="pointer"></dx:ASPxImage></a></td> 
                             <td><a href="Login.aspx"><dx:ASPxImage ID="ASPxImage4" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/exit.png" Width="150px" Height="150px" Title="LOG OUT" Cursor="pointer"></dx:ASPxImage></a></td>   
                         </tr>
                   </table>
                    
                </div>
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>
</asp:Content>
