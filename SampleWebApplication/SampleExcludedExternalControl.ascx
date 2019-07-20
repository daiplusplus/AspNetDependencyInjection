<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SampleExcludedExternalControl.ascx.cs" Inherits="ExcludedNamespace.SampleWebApplication.SampleExcludedExternalControl" %>

<fieldset>
    <legend>External User Control - Excluded (Services should be null)</legend>
    
    <p>Service1 is <strong><asp:Literal runat="server" ID="serviceOne" /></strong></p>
    <p>Service1 is <strong><asp:Literal runat="server" ID="serviceTwo" /></strong></p>
</fieldset>