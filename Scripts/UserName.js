$(function () {

    $.ajax({
        type: "GET",
        url: "https://myaccount.aub.edu.lb/StudLogin/GetUserFullName",
        crossDomain: true,
        data: "{}",
        contentType: "application/json",
        dataType: "jsonp",
        success: function (myName) {
            getName(myName);
        }
    });

    function getName(myName) {
        
        $.ajax({
            type: "GET",
            url: "/teachers/testName",
            data: { myName: myName },
            success: function (result) {
                alert("Done!");
            }
        });

    }

});