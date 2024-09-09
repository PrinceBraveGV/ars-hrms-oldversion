// package id.arista.HRM.controllers;
// import jquery;
// class NgMobileController extends Angular {

"use strict";

var app = angular.module('HRISMobile', []);

if (typeof jQuery === "undefined") {
    throw new Error("This module requires jQuery to run.");
}

app.controller('AdminController', ['$scope', function ($scope) {
    $scope.model = "";

    $scope.getidentity = function (no, name) {
        $scope.no = no;
        $scope.name = name;
    }

    $scope.getprivilege = function (privilege) {
        $scope.privilege = privilege;
    }

    $scope.count = function (arr) {
        $scope.result = arr.length;
    }
}]);

app.controller('ManagerController', ['$scope', function ($scope) {
    $scope.model = "";
}]);

app.controller('UserController', ['$scope', function ($scope) {
    $scope.model = "";
}]);

app.filter('', function () {
    return function () {

    }
});

// }