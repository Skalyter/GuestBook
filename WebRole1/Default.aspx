<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebRole1.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Windows Azure GuestBook</title>
   
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <div class="general">
            <div class="title">
                <h1> Windows Azure GuestBook </h1>
            </div>
            <div class="inputSection">
                <dl>
                    <dt>
                        <label for="NameLabel"> Name:</label></dt>
                    <dd>
                        <asp:TextBox
                            ID="TextBox2"
                            runat="server" />
                        <asp:RequiredFieldValidator
                            ID="NameRequireValidator"
                            runat="server"
                            ControlToValidate="TextBox2"
                            Text=" " />
                    </dd>
                    <dt>
                        <label for="MessageLabel">Message: </label>

                    </dt>
                    <dd>
                        <asp:TextBox
                            ID="TextBox1"
                            runat="server"
                            TextMode="MultiLine" />
                        <asp:RequiredFieldValidator
                            ID="MessageRequiredValidator"
                            runat="server"
                            ControlToValidate="TextBox1"
                            Text =""/>
                        <asp:FileUpload ID="FileUpload1" runat="server" />
                    </dd>
                    </dl>
                    </div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:DataList
                        ID="DataList1"
                        runat="server"
                        DataSourceID="ObjectDataSource1">
                        <ItemTemplate>
                            <div class="signature">
                                <div class="signatureImage">
                                   <a href="<%# Eval("PhotoUrl") %>" target="_blank">
                                       <img src ="<%# Eval("ThumbnailUrl") %>"
                                            alt ="<%# Eval("GuestName") %>"/>

                                   </a>
                                    </div>
                                <div class="signatureDescription">
                                    <div class="signatureName">
                                        <%#Eval ("GuestName") %>
                                    </div>
                                    <div class="signatureSays">
                                      
                                    </div>
                                    <div class="signatureDate">
                                        <%#((DateTimeOffset)Eval("Timestamp")).ToString() %>

                                    </div>
                                    <div class="signatureMessage">
                                         <%#Eval ("Message") %>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:Timer
                        ID="Timer1"
                        runat="server"
                        Interval="15000"
                        OnTick="Timer1_Tick">

                    </asp:Timer>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:ObjectDataSource
                ID="ObjectDataSource1"
                runat="server"
                DataObjectTypeName="GuestBook_Data.GuestBookEntry"
                SelectMethod="GetGuestBookEntries"
                TypeName="GuestBook_Data.GuestBookDataSource">

            </asp:ObjectDataSource>
            </div>
        </form>
    <</body>
    </html>


                    