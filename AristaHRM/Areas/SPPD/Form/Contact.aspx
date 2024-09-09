<%@ Page Title="Contact Us" Language="C#" MasterPageFile="~/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="SPD.Form.Contact" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>





<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="CONTACT US" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True" Font-Bold="True" View="GroupBox">
        <PanelCollection>
            <dx:PanelContent>
                <div>
                    <table style="padding-left:100px; padding-top:30px; padding-bottom:30px;">
                        <tr>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel6" runat="server" Text="Nama" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td style="width:20px;">
                                <dx:ASPxLabel ID="ASPxLabel7" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtnama" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>
                                <%--<dx:ASPxComboBox ID="ASPxComboBox1" runat="server" TextField="Nama_Karyawan" ValueField="NIK" ValueType="System.String" AutoPostBack="true" DataSourceID="SqlDataSource1" OnValueChanged="ASPxComboBox1_ValueChanged" TextFormatString="{1}">
                                    <Columns>
                                        <dx:ListBoxColumn FieldName="NIK" Caption="NIK" />
                                        <dx:ListBoxColumn FieldName="Nama_Karyawan" Caption="Nama" />
                                    </Columns>
                                </dx:ASPxComboBox>
                                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:SPPD_HRDConnectionString %>" SelectCommand="SELECT [Nik],[Nama_Karyawan] FROM [172.16.110.116].[HRIS_Dev].[dbo].[TM_Karyawan]"></asp:SqlDataSource>--%>

                            </td>
                            <td rowspan="15"><dx:ASPxImage ID="ASPxImage3" runat="server" ShowLoadingImage="true" ImageUrl="~/Image/contact.png" Width="300px" Height="250px" Cursor="pointer"></dx:ASPxImage></td> 
                             
                        </tr>
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="NIK" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel4" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtnik" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel14" runat="server" Text="Email" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel15" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtemail" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel13" runat="server" Text="Perusahaan" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel16" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtPT" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>
                            </td>
                        </tr>
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel5" runat="server" Text="Jabatan" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel8" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtjabatan" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel9" runat="server" Text="Divisi" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel10" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtdivisi" runat="server" Width="170px" Enabled="false" ForeColor="Black"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        
                        <tr style="height:25px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel52" runat="server" Text="Tanggal" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel53" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxDateEdit ID="cmbtanggal" runat="server" Theme="DevEx" EditFormatString="d MMM yyyy" NullText="--Pilih--" Enabled="false" ForeColor="Black" BackColor="#e6e6e6">
                                    <DropDownButton>
                                        <Image IconID="actions_add_16x16" Url="../Gambar/KalenderDua.png" Height="14px" Width="14px"></Image>
                                    </DropDownButton>
                                    <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidateOnLeave="true" SetFocusOnError="true">
                                        <RequiredField IsRequired="true" />
                                    </ValidationSettings>
                                    <InvalidStyle BackColor="LightSkyBlue"></InvalidStyle>
                                </dx:ASPxDateEdit>
                            </td>
                        </tr>

                        <tr>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel19" runat="server" ForeColor="Black" Text="Pesan" ></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel20" runat="server" ForeColor="Black" Text=":"></dx:ASPxLabel>
                            </td>
                            <td>
                                
                                <dx:ASPxTextBox ID="txtpesan" runat="server" Width="170px" Height="80px">
                                    <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidateOnLeave="true" SetFocusOnError="true">
                                        <RequiredField IsRequired="true" />
                                    </ValidationSettings>
                                </dx:ASPxTextBox>
                            </td>
                        </tr>
                        
                        <tr style="height:20px;"></tr>
                        
                        <tr style="padding-top:30px;">
                            <td></td>
                            <td></td>
                            <td>
                                <dx:ASPxButton ID="btnsimpan" runat="server" Text="SUBMIT" Theme="Office2010Silver" AutoPostBack="false" CausesValidation="true" Font-Size="Small" OnClick="btnsimpan_Click" Style="height: 20px"></dx:ASPxButton>
                                &nbsp;
                                <a href="spd1.aspx" onclick="window.history.go(-2); return false;"> CANCEL</a>
                             </td>
                        </tr>
                        <tr>
                            <td>
                                <dx:ASPxLabel ID="lblMode" runat="server" Font-Size="X-Small" Font-Bold="True"></dx:ASPxLabel>
                            </td>
                            <td></td>
                            <td>
                                <dx:ASPxLabel ID="lblError" runat="server" ForeColor="Red" Font-Size="X-Small" Font-Bold="True"></dx:ASPxLabel>
                            </td>
                        </tr>
                    </table>
                </div>
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>

</asp:Content>
