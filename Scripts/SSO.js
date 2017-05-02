$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: "https://myaccount.aub.edu.lb/StudLogin/GetUserFullName",
        crossDomain: true,
        data: "{}",
        contentType: "application/json",
        dataType: "jsonp",
        success: function (myName) {
            // Replace the div's content with the page method's return.
            if (myName == null) {
                $("#SSO_SignInSignOut").html("<a href='javascript:SSOSignIn();'>Sign In</a>");
                if (window.navigator.userAgent.indexOf("MSIE ") === -1 || window.navigator.userAgent.indexOf("compatible") === -1) {
                    SSOSignOut(true);//when ie is in compatibility mode myName is returned null
                }
            }
            else
                $("#SSO_SignInSignOut").html("<small>You are logged in as<br/> " + myName + "<br/> <a href='javascript:SSOSignOut(false);'>Sign Out</a></small>");
            if (window.location.href.indexOf('SSOId') > -1 && history.pushState) {
                var newurl = window.location.href.substring(0, window.location.href.indexOf('SSOId') - 1);
                window.history.pushState({ path: newurl }, '', newurl);
            }
        },
        error: function () {
            $("#SSO_SignInSignOut").html("<a href='javascript:SSOSignIn();'>Sign In</a>");
            SSOSignOut(true); //IE is appending a head and body when the return value is null, so we are getting an error
        }
    });
});
function SSOSignOut(localOnly) {
    var returnUrl = "/"; //for local urls start the returnUrl with / (Ex:/Default.aspx)
    var yourAppName = "BobTheGrader"; //Put the same name that you used in the forms authentication in web.config
    if (localOnly == false) {
        window.location = GetUrl() + "/SSO_Signout?ReturnUrl=" + encodeURIComponent(returnUrl);
    }
    else {
        //This is the Single Sign Out: Only signout if the local authentication cookie is still present. 
        if (document.cookie.indexOf("LoggedInTo" + yourAppName) != -1)
            window.location = GetUrl() + "/SSO_Signout?ReturnUrl=" + encodeURIComponent(returnUrl) + "&LocalOnly=true";
    }
}
function SSOSignIn() {
    window.location = 'https://myaccount.aub.edu.lb/StudLogin/SignIn?ReturnUrl=' + encodeURIComponent(window.location);
}
function GetUrl() {
    var newURL = window.location.protocol + "//" + window.location.host;
    var pathArray = window.location.pathname.split('/');
    for (i = 0; i < pathArray.length; i++) {
        if (pathArray[i] != "") {
            newURL += "/";
            newURL += pathArray[i];
        }
    }
    return newURL;
}
