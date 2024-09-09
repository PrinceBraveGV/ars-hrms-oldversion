<%@ Page Title="Data Master" Language="C#" MasterPageFile="~/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="MasterKaryawan.aspx.cs" Inherits="SPD.Form.MasterKaryawan" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>






<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="Data Master Karyawan" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True">
        <PanelCollection>
            <dx:PanelContent>
                <div>
                    <table>
                        <tr>
                            <td>
                                <dx:ASPxButton ID="newkaryawan" runat="server" Text="TAMBAH DATA" OnClick="newkaryawan_Click"></dx:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </div>
                <dx:ASPxGridView ID="datakaryawan" runat="server" KeyFieldName="NIK">

                    <Columns>

                        <dx:GridViewCommandColumn SelectAllCheckboxMode="Page" ShowSelectCheckbox="True" ShowClearFilterButton="true" VisibleIndex="0" Width="30px">
                            <CellStyle>
                                <Border BorderStyle="None" />
                            </CellStyle>
                        </dx:GridViewCommandColumn>
                        <%--<dx:GridViewDataTextColumn FieldName="id_karyawan" VisibleIndex="1" Caption="id_karyawan" Visible="false">
                        </dx:GridViewDataTextColumn>--%>
                        <dx:GridViewDataTextColumn FieldName="NIK" VisibleIndex="2" Caption="NIK">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Nama_Karyawan" VisibleIndex="3" Caption="Nama_Karyawan">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Email_Perusahaan" VisibleIndex="6" Caption="Email_Perusahaan">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Perusahaan" VisibleIndex="7" Caption="Perusahaan">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Cabang" VisibleIndex="8" Caption="Cabang">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Jabatan" VisibleIndex="9" Caption="Jabatan">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Departemen" VisibleIndex="10" Caption="Departemen">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Status_Karyawan" VisibleIndex="13" Caption="Status_Karyawan">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Status_Kerja" VisibleIndex="14" Caption="Status_Kerja">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Area_Kerja" VisibleIndex="15" Caption="Area_Kerja">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Last_Login" VisibleIndex="16" Caption="Last_Login">
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Last_Session" VisibleIndex="17" Caption="Last_Session">
                        </dx:GridViewDataTextColumn>
                        
                        <dx:GridViewDataTextColumn Caption="Option" Name="Option" VisibleIndex="18" Width="80px">
                                <DataItemTemplate>
                                    <asp:LinkButton ID="klikedit" runat="server" OnClick="klikedit_Click"> Edit </asp:LinkButton>
                                    |
                                    <asp:LinkButton ID="klikdelete" runat="server" OnClick="klikdelete_Click"> Delete </asp:LinkButton>
                                </DataItemTemplate>

                            </dx:GridViewDataTextColumn>
                    </Columns>
                </dx:ASPxGridView>

            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>
</asp:Content>