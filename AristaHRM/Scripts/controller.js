// package id.arista.HRM.controllers;
// import com.jquery.*;
// import net.angular.*;
// class NgController extends Angular {

"use strict";

var app = angular.module('HRIS', []);

if (typeof jQuery === "undefined") {
    throw new Error("This module requires jQuery to run.");
}

app.controller('AdminController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.privilege = "";

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

app.controller('HomeController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.privilege = "";

    $scope.getidentity = function (no, name) {
        $scope.no = no;
        $scope.name = name;
    }

    $scope.getprivilege = function (privilege) {
        $scope.privilege = privilege;
    }

}]);

app.controller('InputController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.privilege = "";

    $scope.getidentity = function (no, name) {
        $scope.no = no;
        $scope.name = name;
    }

    $scope.getprivilege = function (privilege) {
        $scope.privilege = privilege;
    }

    $scope.settarget = function (user) {
        $scope.target = user;
    }

}]);

app.controller('ManagerController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.privilege = "";
    $scope.message = "";
    $scope.subordinate = [];

    $scope.getidentity = function (no, name) {
        $scope.no = no;
        $scope.name = name;
    }

    $scope.getprivilege = function (privilege) {
        $scope.privilege = privilege;
    }

    $scope.getlistbawahan = function (list) {
        if (typeof list !== 'undefined') {
            for (var i = 0; i < list.length; i++) {
                $scope.subordinate[i] = list[i];
            }
        } else {
            $scope.message = "Tidak ada bawahan untuk karyawan ybs.";
        }
    }

    $scope.getapproved = function (list) {
        if (typeof list !== 'undefined') {
            for (var i = 0; i < list.length; i++) {
                $scope.approved[i] = list[i];
            }
        } else {
            $scope.message = "Tidak ada pengajuan yang disetujui untuk atasan ybs.";
        }
    }

}]);

app.controller('MasterController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.message = "";

}]);

app.controller('SupervisorController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";

    $scope.getlistbawahan = function (list) {
        if (typeof list !== 'undefined') {
            for (var i = 0; i < list.length; i++) {
                $scope.subordinate[i] = list[i];
            }
        } else {
            throw new Error("Tidak ada bawahan untuk karyawan ybs.");
        }
    }

}]);

app.controller('UserController', ['$scope', function ($scope) {
    // Model binding
    $scope.model = "";
    $scope.message = "";

}]);

// Filtering method
// Sintaks filter: {{ kata kunci value | kata kunci key }}
app.filter('check', function () {
    return function (value) {
        // return value di sini
        return "";
    }
});

app.filter('mode', function () {
    return function (mode) {
        return mode === "debug";
    }
});

app.factory('Provider', ['$http', function ($http) {
    var factory = {};

    // HTTP request
    factory.httpRequest = function (url, method, data) {
        $http({
            method: method,
            url: url,
        }).then(function success(response) {

        }, function error(response) {

        });
    }

    // HTTP GET
    factory.getUrl = function (url) {
        return $http.get(url);
    }

    // HTTP POST
    factory.postUrl = function (data, url) {
        $http.post(url, data).success(function (response) {
            // respon hasil posting

        }).error(function (err) {

        });
    }

    // HTTP PUT
    factory.putUrl = function (data, url) {
        $http.put(url, data).success(function (response) {

        });
    }
}]);

// }

