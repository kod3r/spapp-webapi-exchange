'use strict';
angular.module('mailApp')
.controller('homeCtrl', ['$scope', 'adalAuthenticationService','$location', function ($scope, adalService, $location) {
    $scope.isActive = function (viewLocation) {        
        return viewLocation === $location.path();
    };
}]);