'use strict';
angular.module('mailApp')
.controller('mailCtrl', ['$scope', '$location', 'mailSvc', 'adalAuthenticationService', function ($scope, $location, mailSvc, adalService)
{
    $scope.error = "";
    $scope.loadingMessage = "Loading...";
    $scope.mailList = null;

    $scope.populate = function ()
    {
        mailSvc.getItems().success(function (results)
        {
            $scope.mailList = results;
            $scope.loadingMessage = "";
        }).error(function (err)
        {
            $scope.error = err;
            $scope.loadingMessage = "";
        })
    };

}]);