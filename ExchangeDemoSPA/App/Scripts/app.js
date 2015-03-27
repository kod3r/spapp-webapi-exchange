'use strict';
angular.module('mailApp', ['ngRoute','AdalAngular'])
.config(['$routeProvider', '$httpProvider', 'adalAuthenticationServiceProvider', function ($routeProvider, $httpProvider, adalProvider) {

    $routeProvider.when("/Home", {
        controller: "homeCtrl",
        templateUrl: "/App/Views/Home.html",    
    }).when("/Mail", {
        controller: "mailCtrl",
        templateUrl: "/App/Views/Mail.html",
        requireADLogin: true,    
    }).otherwise({ redirectTo: "/Home" });

    var endpoints = {
        
        "https://localhost.fiddler:44350/":
        "https://kirke3.onmicrosoft.com/ExchangeDemoAPI",
    };

    adalProvider.init(
        {
            tenant: 'kirke3.onmicrosoft.com',
            clientId: 'c2b3f3bc-1b6a-4c17-8b89-b5e474dd3949',
            extraQueryParameter: 'nux=1',
            endpoints: endpoints,
            cacheLocation: 'localStorage', // enable this for IE, as sessionStorage does not work for localhost.  
            // Also, token acquisition for the To Go API will fail in IE when running on localhost, due to IE security restrictions.
        },
        $httpProvider
        );
   
}]);
