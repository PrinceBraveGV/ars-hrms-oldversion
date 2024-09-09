<%@ Page Title="About System" Language="C#" MasterPageFile="~/Areas/SPPD/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="Help.aspx.cs" Inherits="SPD.Form.Help" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxMenu" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxSplitter" TagPrefix="dx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="PANDUAN UMUM" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True">
        <PanelCollection>
            <dx:PanelContent>
                <div style="text-align:justify;">
                    <table>
                        <tr>
                            <td>SPD Form 1</td>
                            <td>(FORM PENGAJUAN)</td>
                        </tr>
                        <tr>
                              <td><dx:ASPxImage ID="ASPxImage1" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/datas1.png" Width="110px" Height="100px" Title="SPD Form 1"></dx:ASPxImage></td>
                              <td style="padding-left:10px;"> SPD Form 1 Digunakan untuk pengajuan perjalanan dinas dengan mengisi data pada field/kolom yang telah disediakan pada form tersebut dengan pengajuan UANG MUKA ataupun TIDAK. </td> 
                        </tr>
                        <tr>
                        </tr>
                        <tr>
                            <td>SPD Form 2</td>
                            <td>(FORM PERTANGGUNG-JAWABAN)</td>
                        </tr>
                        <tr>
                              <td><dx:ASPxImage ID="ASPxImage2" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/datas2.png" Width="110px" Height="100px" Title="SPD Form 2"></dx:ASPxImage></td>
                              <td style="padding-left:10px;"> 
                                  SPD Form 2 Digunakan untuk pelaporan terkait dengan perjalanan dinas yang telah dilakukan serta laporan dana yang digunakan selama melakukan perjalanan dinas. SPD Form 2 dapat diisi dengan input Nomor Refrensi Form 1 (Pengajuan) agar data anda dapat terdata dengan baik.
                                  Pengisian Form 2 dilakukan dengan tahap mengisi Data Utama yang terdiri dari Nama,Nik,dll ATAUPUN cukup dengan mengisi No Ref Form 1 jika melakukan pengajuan Form 1 sebelum perjalanan dinas. Setelah Data Utama disimpan maka KLIK ADD DETAIL (kiri bawah form input) Lalu isi keterangan biaya yang telah dikeluarkan selama perjalanan dinas. 
                              </td> 
                        </tr>
                        <tr>
                        </tr>
                        <tr>
                            <td>Contact US</td>
                            <td>(FORM CONTACT ADMIN)</td>
                        </tr>
                        <tr>
                              <td><dx:ASPxImage ID="ASPxImage3" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/contact.png" Width="120px" Height="100px" Title="SPD Form 1"></dx:ASPxImage></td>
                              <td style="padding-left:10px;"> Form Contact Us dapat anda gunakan untuk pengaduan apabila terjadi masalah pada system seperti Bug/Error. Ataupun anda dapat memberikan kritik & saran serta pertanyaan terkait dengan sistem SPD Online kepada Admin Kami.</td>
                         </tr>

                   </table>
                    
                </div>
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>
</asp:Content>
