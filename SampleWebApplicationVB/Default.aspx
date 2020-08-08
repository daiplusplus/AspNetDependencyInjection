<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="SampleWebApplicationVB.DefaultPage" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
	<h2>Welcome to Injected ASP.NET WebForms </h2>
	
	<asp:Literal runat="server" ID="ltMessages" />
	<hr />
</asp:Content>
