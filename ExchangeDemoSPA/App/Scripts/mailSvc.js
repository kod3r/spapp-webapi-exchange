'use strict';
angular.module('mailApp')
.factory('mailSvc', ['$http', function ($http)
{

    //var apiEndpoint = "Enter the root location of your Mail API here, e.g. https://contosotogo.azurewebsites.net/";
    var apiEndpoint = "https://localhost.fiddler:44350/";

    $http.defaults.useXDomain = true;
    delete $http.defaults.headers.common['X-Requested-With'];

    return {
        getItems: function ()
        {
            return $http.get(apiEndpoint + 'api/mail');
        }
    };
}]);