﻿<%@ Page Title="SPD F2 All Data" Language="C#" MasterPageFile="~/Areas/SPPD/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="ALLSPD2.aspx.cs" Inherits="SPD.Form.ALLSPD2" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxPanel" TagPrefix="dx" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxGridView" TagPrefix="dx" %>

<%@ Register assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxEditors" tagprefix="dx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="Laporan Perjalanan Dinas Form 2" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True" style="margin-left: 26px">
        <PanelCollection>
            <dx:PanelContent>
                <div>
                    <table>
                        <tr>
                            <td>
                                <dx:ASPxButton ID="tambahsppd2" runat="server" Text="TAMBAH DATA" OnClick="tambahsppd2_Click"></dx:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </div>
                <div>
                   <dx:ASPxGridView ID="datas" ClientInstanceName="ASPxGridView1" runat="server" AutoGenerateColumns="False" KeyFieldName="Id" Theme="Office2010Blue" Font-Size="10px" EnableTheming="True" Width="1080px">


                        <%--untuk mengambil value dari AspxGridview jangan lupa setting di property:
*SettingsBehavior AllowFocusedRow="True" --%>


                        <SettingsBehavior AllowFocusedRow="True" />
                       
                        <Columns>

                            <dx:GridViewCommandColumn SelectAllCheckboxMode="Page" ShowSelectCheckbox="True" ShowClearFilterButton="true" VisibleIndex="0" Width="30px">
                                <CellStyle>
                                    <Border BorderStyle="None" />
                                </CellStyle>
                            </dx:GridViewCommandColumn>

                            <dx:GridViewDataTextColumn FieldName="Id" VisibleIndex="1" Caption="id" Visible="false">
                            </dx:GridViewDataTextColumn>
                            
                            <dx:GridViewDataTextColumn FieldName="No" VisibleIndex="2" Caption="No">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Nama_Karyawan" VisibleIndex="3" Caption="Nama Karyawan">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="NIK" VisibleIndex="4">
                            </dx:GridViewDataTextColumn>
                            
                            <dx:GridViewDataTextColumn FieldName="PT" VisibleIndex="5" Caption="Perusahaan">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Jabatan" VisibleIndex="6">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Cabang" VisibleIndex="7">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Divisi" VisibleIndex="8" Caption="Departemen">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Jenis_Perjalanan" VisibleIndex="9" Caption="Jenis_Perjalanan" Width="100px">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataDateColumn FieldName="Tanggal_Pengajuan" Caption="Tgl Pengajuan" VisibleIndex="10" PropertiesDateEdit-DisplayFormatString="d MMM yyyy">
                                <PropertiesDateEdit DisplayFormatString="d MMM yyyy"></PropertiesDateEdit>
                            </dx:GridViewDataDateColumn>
                          

                             <dx:GridViewDataTextColumn Caption="Option" Name="Option" VisibleIndex="12" Width="110px">
                                <DataItemTemplate>

                                    <asp:LinkButton ID="klikedit" runat="server" OnClick="klikedit">Edit</asp:LinkButton>
                                    |
                                    <asp:LinkButton ID="klikdelete" runat="server" OnClick="klikdelete">Delete</asp:LinkButton>
                                    |
                                    <asp:LinkButton ID="klikprint" runat="server" OnClick="klikprint">View</asp:LinkButton>

                                </DataItemTemplate>

                            </dx:GridViewDataTextColumn>

                        </Columns>

                       <Styles>
                            <AlternatingRow BackColor="White">
                            </AlternatingRow>
                            <FocusedRow BackColor="#e1f0ff" ForeColor="#000000">
                            </FocusedRow>
                            <Cell>
                                <Border BorderStyle="None" />
                            </Cell>
                        </Styles>
                       
                    </dx:ASPxGridView>
                </div>
                           
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>
</asp:Content>
