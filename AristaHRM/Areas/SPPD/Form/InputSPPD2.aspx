<%@ Page Title="Entry SPD F2" Language="C#" MasterPageFile="~/Areas/SPPD/Master/Left_Site.Master" AutoEventWireup="true" CodeBehind="InputSPPD2.aspx.cs" Inherits="SPD.Form.InputSPPD2" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxGridView" TagPrefix="dx" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dx" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dx" %>

<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxPanel" TagPrefix="dx" %>



<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <dx:ASPxRoundPanel ID="ClientSideGroupBox" runat="server" Width="100%" HeaderText="Input SPD2" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True" Font-Bold="True" View="GroupBox">
        <PanelCollection>
            <dx:PanelContent>
                <div>
                    <table style="padding-left:30px;">
                        
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel2" runat="server" Text="No" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtNo" runat="server" Width="170px" Enabled="false" ForeColor="Black" BackColor="#e6e6e6">
                                </dx:ASPxTextBox>
                               
                            </td>

                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel52" runat="server" Text="No. Reff Form1" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel53" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <%--<dx:ASPxTextBox ID="txtNoReff" runat="server" ForeColor="Black" Width="170px" OnTextChanged="txtNoReff_TextChanged" AutoPostBack="true" ></dx:ASPxTextBox>--%>  
                                <dx:ASPxComboBox ID="ASPxComboBox1" runat="server" TextField="No" ValueField="No" ValueType="System.String" AutoPostBack="true" DataSourceID="SqlDataSource1" OnSelectedIndexChanged="ASPxComboBox1_SelectedIndexChanged">
                                    <Columns>
                                        <dx:ListBoxColumn FieldName="No" Caption="No" />
                                    </Columns>
                                </dx:ASPxComboBox>
                                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:SPPD_HRDConnectionString %>"  SelectCommand="SELECT [No] FROM [SPPD_F1] WHERE ([Nama_Karyawan] = @Nama)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="Nama" SessionField="Nama" Type="String" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </td>

                            <td style="padding-left:-40px;"> 
                                <dx:ASPxLabel ID="ASPxLabel54" runat="server" Text="<-Masukkan Reff Nomor Surat Form1 Jika Melakukan Pelaporan Berdasarkan Form1->" ForeColor="Red"></dx:ASPxLabel>
                            </td>
                        </tr>
                         <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel6" runat="server" Text="Nama" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel7" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtnama" runat="server" Width="170px"  ForeColor="Black" Enabled="false"></dx:ASPxTextBox>
                                <%--<dx:ASPxComboBox ID="ASPxComboBox1" runat="server" TextField="Nama_Karyawan" ValueField="NIK" ValueType="System.String" AutoPostBack="true" DataSourceID="SqlDataSource1" OnValueChanged="ASPxComboBox1_ValueChanged" TextFormatString="{1}">
                                    <Columns>
                                        <dx:ListBoxColumn FieldName="NIK" Caption="NIK" />
                                        <dx:ListBoxColumn FieldName="Nama_Karyawan" Caption="Nama" />
                                    </Columns>
                                </dx:ASPxComboBox>
                                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:SPPD_HRDConnectionString %>" SelectCommand="SELECT [Nik],[Nama_Karyawan] FROM [TM_Karyawan]"></asp:SqlDataSource>--%>

                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="NIK" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel4" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtnik" runat="server" Width="170px" ForeColor="Black" Enabled="false"></dx:ASPxTextBox>

                            </td>
                        </tr>
                         <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel13" runat="server" Text="Perusahaan" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel16" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtPT" runat="server" Width="170px" ForeColor="Black" Enabled="false"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel5" runat="server" Text="Jabatan" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel8" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtjabatan" runat="server" Width="170px" ForeColor="Black"  Enabled="false"></dx:ASPxTextBox>

                            </td>
                        </tr>
                         <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel9" runat="server" Text="Divisi" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel10" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtdivisi" runat="server" Width="170px"  ForeColor="Black" Enabled="false"></dx:ASPxTextBox>

                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel11" runat="server" Text="Cabang" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel12" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtcabang" runat="server" Width="170px" ForeColor="Black" Enabled="false"></dx:ASPxTextBox>
                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel25" runat="server" Text="Tgl Pengajuan SPD" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel26" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxDateEdit ID="cmbtglpengajuan" runat="server" Theme="DevEx" EditFormatString="d MMM yyyy" NullText="--Pilih--" Enabled="false" ForeColor="Black" BackColor="#e6e6e6">
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
                         <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel14" runat="server" Text="Tgl Berangkat" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel15" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxDateEdit ID="cmbtglberangkat" runat="server" Theme="DevEx" EditFormatString="d MMM yyyy" NullText="--Pilih--" ForeColor="Black">
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
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel21" runat="server" Text="Tgl Pulang" ForeColor="Black" Font-Size="11px"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel22" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxDateEdit ID="cmbtglpulang" runat="server" Theme="DevEx" EditFormatString="d MMM yyyy" NullText="--Pilih--" ForeColor="Black" OnDateChanged="cmbtglpulang_DateChanged" AutoPostBack="true">
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
                        
                          <tr style="margin-bottom:20px;">

                            <td>
                                <dx:ASPxLabel ID="ASPxLabel23" runat="server" ForeColor="Black" Text="Jenis Perjalanan"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel24" runat="server" ForeColor="Black" Text=":"></dx:ASPxLabel>
                            </td>
                            <td>
                                <asp:DropDownList ID="tipeperjalanan" runat="server">
                                    <asp:ListItem Value="Menginap"> Menginap </asp:ListItem>
                                    <asp:ListItem Value="Tidak Menginap"> Tidak Menginap </asp:ListItem>
                                </asp:DropDownList>
                                
                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel17" runat="server" Text="Jumlah_Hari" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel18" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtjumlahhari" runat="server" Width="50px"></dx:ASPxTextBox>
                            </td>
                            <%--<dx:ASPxButton ID="IncreaseBtn" runat="server" Text="+" OnClick="IncreaseBtn_Click"></dx:ASPxButton>
                            <dx:ASPxButton ID="DecreaseBtn" runat="server" Text="-" OnClick="DecreaseBtn_Click"></dx:ASPxButton>--%>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel19" runat="server" Text="Uang Muka Yang Diterima" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxLabel ID="ASPxLabel20" runat="server" Text=":" ForeColor="Black"></dx:ASPxLabel>
                            </td>
                            <td>
                                <dx:ASPxTextBox ID="txtuangmuka" runat="server" Width="170px"></dx:ASPxTextBox>
                            </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
                            <td></td>
                            <td></td>
                            <td>
                                <dx:ASPxButton ID="btnsimpan" runat="server" Text="SUBMIT" Theme="Office2010Silver" AutoPostBack="True" CausesValidation="true" Font-Size="Small" OnClick="simpan_Click" Style="height: 20px" Enabled="true"></dx:ASPxButton>
                                &nbsp;
                                <a href="spd2.aspx" onclick="window.history.go(-2); return false;"> CANCEL</a>
                             </td>
                        </tr>
                        <tr style="margin-bottom:20px;">
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

    <dx:ASPxRoundPanel ID="ASPxRoundPanel1" runat="server" Width="100%" HeaderText="Input Detail SPD2" HorizontalAlign="Left" Theme="SoftOrange" ShowCollapseButton="True" Font-Bold="True" View="GroupBox">
        <PanelCollection>
            <dx:PanelContent>
                <div>
                    <table>
                        <tr>
                            <td>
                                <dx:ASPxButton ID="AddDetail" runat="server" Text="Add Detail" OnClick="AddDetail_Click"></dx:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </div>
                <div>
                    <table>
                        <tr>
                            <td>
                                <dx:ASPxGridView ID="datadetail" AutoGenerateColumns="False" KeyFieldName="Id_detail" Theme="Office2010Blue" Font-Size="10px" EnableTheming="True" runat="server">
                                  
                                <SettingsBehavior AllowFocusedRow="True" />
                       
                        <Columns>

                            <dx:GridViewCommandColumn SelectAllCheckboxMode="Page" ShowSelectCheckbox="True" ShowClearFilterButton="true" VisibleIndex="0" Width="30px">
                                <CellStyle>
                                    <Border BorderStyle="None" />
                                </CellStyle>
                            </dx:GridViewCommandColumn>

                            <dx:GridViewDataTextColumn FieldName="Id_detail" VisibleIndex="1" Visible="False">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="No" VisibleIndex="2" Visible="false">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Keterangan" VisibleIndex="3" >
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Tanggal" VisibleIndex="4">

                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Biaya_Tranport" VisibleIndex="5"  Caption="Biaya Tranport">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Biaya_Penginapan" VisibleIndex="6" Caption="Biaya Penginapan">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Biaya_Makan" VisibleIndex="7" Caption="Biaya Makan">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Biaya_Saku" VisibleIndex="8" Caption="Biaya Saku">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Biaya_Lain" VisibleIndex="9" Caption="Biaya Lain">
                            </dx:GridViewDataTextColumn>

                            <dx:GridViewDataTextColumn FieldName="Jumlah" VisibleIndex="10" Caption="Jumlah">
                            </dx:GridViewDataTextColumn>

                             <dx:GridViewDataTextColumn Caption="Option" Name="Option" VisibleIndex="11" Width="110px">
                                <DataItemTemplate>

                                   <asp:LinkButton ID="klikedit" runat="server" OnClick="klikedit">Edit</asp:LinkButton>
                                    |
                                    <asp:LinkButton ID="klikhapus" runat="server"  OnClick="klikhapus">Delete</asp:LinkButton>
                               

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
                            </td>
                        </tr>
                    </table>
                </div>
            </dx:PanelContent>
        </PanelCollection>
    </dx:ASPxRoundPanel>

</asp:Content>
