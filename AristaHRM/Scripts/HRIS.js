// package js;
// import com.jquery.*;

// public class JSExtension extends jQuery
// {

(function (window, global, factory) {
    // assume CommonJS exists
    typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory(global, true) :
    typeof define !== 'undefined' && typeof define === 'function' && define.amd ? define("js", [], function () { return js; }) : global.js = factory()

    var target = (typeof exports === 'object' && typeof module === 'object') ? module['exports'] || exports : define(['exports'], factory);
}(typeof window !== "undefined" ? window : this, this, function () {
    'use strict';

    var js = function (context) {
        return js.prototype.init();
    }

    if (typeof jQuery === "undefined") {
        throw new Error("JSExtension library requires jQuery to run.");
    }

    js.props = {};

    var init = js.props.init = function (context) {
        return context;
    }

    // Versioning
    js.version = "1.0.0.0";

    js.getVersion = function () {
        return js.version;
    };

    js.proto = js.prototype = {
        jsbase: js.version,
        constructor: js,
        length: 0,
        get: function () {

        },
        set: function () {

        },
        forEach: function (call) {
            return js.forEach(this, call);
        },
        indexOf: function () {

        },
        init: function () {

        },
        pop: function (stack) {
            if (typeof stack !== "array") {
                return false;
            }
            return stack.pop();
        },
        push: function (stack, content) {
            if (typeof stack !== "array") {
                return false;
            }
            return stack.push(content);
        },

    };

    var hooker;

    function utilhook() {
        return hooker.apply(null, arguments);
    }

    js.domEval = function (code, page) {
        page = page || document;
        var script = page.createElement("script");
        script.text = code;
        page.head.appendChild(script).parentNode.removeChild(script);
    }

    if (!(js.lang && js.io && js.net && js.nio && js.util)) {
        js.lang = {};
        js.io = {};
        js.net = {};
        js.nio = {};
        js.util = {};

        // special packages
        js.awt = {};
    }

    js.debug = true;

    js.extends = function (a, b) {
        for (var i in b) {
            if (hasOwnProp(b, i)) {
                a[i] = b[i];
            }
        }

        if (hasOwnProp(b, 'toString')) {
            a.toString = b.toString;
        }

        if (hasOwnProp(b, 'valueOf')) {
            a.valueOf = b.valueOf;
        }

        return a;
    };

    js.implements = function (a, b) {

    };

    js.throws = function () {
        throw new js.lang.Throwable.getError();
    }

    js.export = function (path, obj) {
        var tokens = path.split(".");

        var target = js;

        for (var i = 0; i < tokens.length - 1; i++) {
            target = target[tokens[i]];
        }
        target[tokens[tokens.length - 1]] = object;
    };

    // Language property culture descriptor.
    js.lang.culture = function (selector) {
        return new js.initalize.prototype.init(selector);
    }

    js.lang.cultures = {};

    js.lang.culture.prototype = {
        constructor: js.lang,
        init: function (selector) {
            this.cultures = js.lang.cultures;
            this.selector = selector;

            return this;
        }
    };

    // Standard language libraries (primitive types & reference types)
    js.lang.Boolean = {}; // boolean
    js.lang.Byte = {}; // byte
    js.lang.Character = {}; // char
    js.lang.Class = {}; // special class functions
    js.lang.Double = {}; // double
    js.lang.Float = {}; // float
    js.lang.Long = {}; // long
    js.lang.Integer = {}; // int
    js.lang.Object = {}; // Object
    js.lang.Short = {}; // short
    js.lang.String = {}; // String

    // Other language libraries
    js.lang.Exception = {}; // Exception class
    js.lang.System = {}; // System class
    js.lang.Throwable = {}; // Throwable class

    // I/O classes
    js.io.FileUpload = {};
    js.io.FileReader = {}; // File reader class
    js.io.FileWriter = {}; // File writer class

    // Network classes
    js.net.Connection = {};
    js.net.Manifest = {};
    js.net.Json = {};
    js.net.URL = {};

    // NIO classes
    js.nio.Buffer = {};
    js.nio.ByteBuffer = {};
    js.nio.ByteOrder = {};

    // Utility libraries
    js.util.ArrayList = {};
    js.util.Date = {};
    js.util.HashMap = {};
    js.util.HashSet = {};
    js.util.Math = {};
    js.util.Queue = {};
    js.util.Stack = {};

    // Abstract window toolkit libraries
    js.awt.Color = {};

    /*
     *  Language base methods & properties.
     *  Package: js.lang
     */

    // Throwable class.
    js.lang.Throwable = {
        getError: function () {
            throw new Error();
        },

        getMessage: function (e) {
            return e.message;
        },

        getStackTrace: function () {
            return error.stack || error.stacktrace;
        },

        printStackTrace: function () {
            var error = new Error();
            console.trace();
            return console.log(error.stack || error.stacktrace || "");
        },

        setStackTrace: function (err) {
            var error = err;
            console.trace();
            return console.log(err);
        }
    };

    // Boolean methods.
    js.lang.Boolean = {
        // Compare 2 boolean values returning negative, zero or positive number.
        compare: function (x, y) {
            if (typeof x !== "boolean" && typeof y !== "boolean") {
                return -1;
            }
            else {
                if (x === y)
                    return 0;
                else if (!x && y)
                    return -1;
                else if (x && !y)
                    return 1;
            }
        },

        // Compare 2 boolean values extensively returning negative, zero or positive number.
        compareTo: function (a, b) {
            if (typeof a !== "boolean" && typeof b !== "boolean") {
                return -1;
            }
            else {
                if (a === b)
                    return 0;
                else if (a > b || (a === true && b === false))
                    return 1;
                else if (a < b || (a === false && b === true))
                    return -1;
            }
        },

        // Check equality between 2 boolean values.
        equals: function (x, y) {
            return !!(x === y);
        },

        // Get boolean property from an object.
        getBoolean: function (name) {
            var result = false;
            try {
                result = js.lang.Boolean.parseBoolean(name);
            }
            catch (e) {
                js.lang.Throwable.setStackTrace(e);
            }
        },

        // Get hash code.
        hashCode: function (x) {
            return x ? 1231 : 1237;
        },

        // Check variable declaration type against boolean type.
        isBoolean: function (input) {
            return input instanceof Boolean;
        },

        // Parse input value as boolean value.
        parseBoolean: function (s) {
            if (s != null && js.lang.String.equals(s.toLowerCase(), "true")) {
                return true;
            }
            else
                return false;
        },

        // Set a boolean value to boolean-type variable.
        setBoolean: function (bool) {
            return (typeof bool === "boolean") ? bool : false;
        },

        // @Override
        toString: function (input) {
            return input ? "true" : "false";
        },

        // Get boolean representation of certain variable.
        valueOf: function (str) {
            return js.lang.Boolean.parseBoolean(str) ? true : false;
        }
    };

    // Signed byte constructor.
    js.lang.Byte.constructor = function (context) {
        return context;
    }

    // Signed byte methods.
    js.lang.Byte = {
        decode: function () {

        },

        parseByte: function (input) {
            if (input < js.lang.Byte.MIN_VALUE || input > js.lang.Byte.MAX_VALUE) {
                throw js.lang.Exception.OutOfRangeException;
            }
            else if (typeof input === "undefined") {
                throw js.lang.Exception.UndefinedException;
            }
            else {
                return parseInt(input);
            }
        },

        MAX_VALUE: 32767,
        MIN_VALUE: -32768,

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toOctalString: function (input) {
            return js.lang.String.toOctalString(input);
        },

        toString: function (input) {
            return input.toString();
        },

        valueOf: function (str) {
            var temp = parseInt(str);
            if (temp < js.lang.Byte.MIN_VALUE) { temp = js.lang.Byte.MIN_VALUE }
            else if (temp > js.lang.Byte.MAX_VALUE) { temp = js.lang.Byte.MAX_VALUE }
            return temp;
        }
    };

    // Character constructor.
    js.lang.Character.constructor = function (context) {
        return context;
    }

    // Character (char) methods.
    js.lang.Character = {
        isDigit: function (chr) {
            return typeof chr === "number";
        },

        isLetter: function (chr) {
            return typeof chr === "string";
        },

        isLetterOrDigit: function (chr) {
            return typeof chr === "string" || typeof chr === "number";
        },

        isLowerCase: function (chr) {
            return chr === chr.toLowerCase();
        },

        isUpperCase: function (chr) {
            return chr === chr.toUpperCase();
        },

        isWhiteSpace: function (chr) {
            return chr === " ";
        },

        toLower: function (chr) {
            return js.lang.String.toLower(chr);
        },

        toUpper: function (chr) {
            return js.lang.String.toUpper(chr);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    // Class special methods.
    js.lang.Class = {
        isArray: function (input) {
            return input instanceof Array || Object.prototype.toString.call(input) === '[object Array]';
        },

        isObject: function (input) {
            return Object.prototype.toString.call(input) === '[object Object]';
        },

        // Check primitive types (byte, char, int, double, float, long)
        isPrimitive: function (input) {
            return input instanceof Number || input instanceof Boolean || typeof input === "number" || typeof input === "boolean";
        },
    };

    // Double constructor.
    js.lang.Double.constructor = function (context) {
        return context;
    };

    // Double methods.
    js.lang.Double = {
        isInfinite: function (input) {
            return isFinite(input) ? false : true;
        },

        isNaN: function (input) {
            return isNaN(input);
        },

        MAX_VALUE: Number.MAX_VALUE,
        MIN_VALUE: Number.MIN_VALUE,

        parseDouble: function (input) {
            if (input < js.lang.Double.MIN_VALUE || input > js.lang.Double.MAX_VALUE) {
                throw js.lang.Exception.OutOfRangeException;
            }
            else {
                return parseFloat(input);
            }
        },

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toOctalString: function (input) {
            return js.lang.String.toOctalString(input);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    // Float constructor.
    js.lang.Float.constructor = function (context) {
        return context;
    };

    // Float methods.
    js.lang.Float = {
        isInfinite: function (input) {
            return isFinite(input) ? true : false;
        },

        isNaN: function (input) {
            return isNaN(input);
        },

        MAX_VALUE: parseFloat("3.402823E+38"),
        MIN_VALUE: parseFloat("-3.402823E+38"),

        parseFloat: function (input) {
            if (input < js.lang.Float.MIN_VALUE || js.lang.Float.MAX_VALUE) {
                throw js.lang.Exception.OutOfRangeException;
            }
            else {
                return parseFloat(input);
            }
        },

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toOctalString: function (input) {
            return js.lang.String.toOctalString(input);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    // Integer constructor.
    js.lang.Integer.constructor = function (context) {
        return context;
    }

    // Integer methods.
    js.lang.Integer = {
        MAX_VALUE: 2147483647,
        MIN_VALUE: -2147483648,

        parseInt: function (input) {
            if (input < js.lang.Integer.MIN_VALUE || input > js.lang.Integer.MAX_VALUE) {
                throw js.lang.Exception.OutOfRangeException;
            }
            else {
                return parseInt(input);
            }
        },

        reverse: function (input) {

        },

        reverseBytes: function (input) {

        },

        rotateLeft: function (input, size) {
            return input << size;
        },

        rotateRight: function (input, size) {
            return input >> size;
        },

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toOctalString: function (input) {
            return js.lang.String.toOctalString(input);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    js.lang.Long.constructor = function (context) {
        return context;
    }

    js.lang.Long = {
        MAX_VALUE: parseFloat("9.2233E+18"),
        MIN_VALUE: parseFloat("-9.2233E+18"),
        parseLong: function (input) {
            if (input < js.lang.Long.MIN_VALUE || input > js.lang.Long.MAX_VALUE) {
                throw js.lang.Exception.OutOfRangeException;
            } else {
                return parseInt(input);
            }
        },

        reverse: function () {

        },

        reverseBytes: function () {

        },

        rotateLeft: function (input, size) {
            return input << size;
        },

        rotateRight: function (input, size) {
            return input >> size;
        },

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toOctalString: function (input) {
            return js.lang.String.toOctalString(input);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    js.lang.Object.constructor = function (context) {
        return context;
    }

    js.lang.Object = {
        assert: function () {

        },

        equals: function (objX, objY) {
            return objX == objY;
        },

        hasProperty: function (prop) {
            return Object.hasOwnProperty(prop)
        },

        isPrototype: function (obj) {
            return Object.isPrototypeOf(obj);
        },

        referenceEquals: function (a, b) {
            return a === b;
        },
    };

    js.lang.Short = {
        MAX_VALUE: 127,
        MIN_VALUE: -128,
        parseShort: function (input) {
            if (input > MAX_VALUE && input < MIN_VALUE) {
                return js.lang.Exception
            } else {
                return parseInt(input);
            }
        },

        reverse: function () {

        },

        reverseBytes: function () {

        },

        toBinaryString: function (input) {
            return js.lang.String.toBinaryString(input);
        },

        toHexString: function (input, padding, ucase) {
            return js.lang.String.toHexString(input, padding, ucase);
        },

        toString: function (input) {
            return input.toString();
        }
    };

    js.lang.String.constructor = function (context) {
        return context;
    }

    js.lang.String = {
        charAt: function (str, int) {
            return str.charAt(int);
        },

        compareTo: function (x, y) {
            if (typeof y !== "string") {
                return -1;
            }
            else {
                return x.localeCompare(y);
            }
        },

        compareToIgnoreCase: function (x, y) {
            if (typeof y !== "string") {
                return -1;
            }
            else {
                return x.toLowerCase().localeCompare(y.toLowerCase());
            }
        },

        concat: function (strA, strB) {
            return strA.concat(strB);
        },

        contains: function (str, word) {
            return str.contains(word);
        },

        decodeFromUTF8: function (str) {
            var output = "";
            var i = 0;
            var c = 0, c1 = 0, c2 = 0;

            while (i < str.length) {
                c = str.charCodeAt(i);

                if (c < 128) {
                    output += String.fromCharCode(c);
                    i++;
                }
                else if ((c > 191) && (c < 224)) {
                    c2 = str.charCodeAt(i + 1);
                    output += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                    i += 2;
                }
                else {
                    c2 = str.charCodeAt(i + 1);
                    c3 = str.charCodeAt(i + 2);
                    output += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                    i += 3;
                }
            }
            return output;
        },

        // Returns empty string.
        empty: '',

        encodeToUTF8: function (str) {
            str = str.replace(/\r\n/g, "\n");
            var text = '';

            for (var n = 0; n < str.length; n++) {

                var c = str.charCodeAt(n);

                if (c < 128) {
                    text += String.fromCharCode(c);
                }
                else if ((c > 127) && (c < 2048)) {
                    text += String.fromCharCode((c >> 6) | 192);
                    text += String.fromCharCode((c & 63) | 128);
                }
                else {
                    text += String.fromCharCode((c >> 12) | 224);
                    text += String.fromCharCode(((c >> 6) & 63) | 128);
                    text += String.fromCharCode((c & 63) | 128);
                }
            }
            return text;
        },

        endsWith: function (s, suf) {
            return s.endsWith(suf);
        },

        // Compares string y to specified object x.
        equals: function (x, y) {
            if (typeof x !== "string") {
                return false;
            }
            else {
                if (x === y) {
                    return true;
                }
                else {
                    return false;
                }
            }
        },

        equalsIgnoreCase: function (x, y) {
            if (typeof x !== "string") {
                return false;
            }
            else {
                if (x.toLowerCase() === y.toLowerCase()) {
                    return true;
                }
                else {
                    return false;
                }
            }
        },

        fromBase64String: function (str) {
            var output = "";
            var chr1, chr2, chr3;
            var enc1, enc2, enc3, enc4;
            var i = 0;

            input = str.replace(/[^A-Za-z0-9\+\/\=]/g, "");

            while (i < input.length) {
                enc1 = base64.indexOf(input.charAt(i++));
                enc2 = base64.indexOf(input.charAt(i++));
                enc3 = base64.indexOf(input.charAt(i++));
                enc4 = base64.indexOf(input.charAt(i++));

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                output = output + String.fromCharCode(chr1);

                if (enc3 != 64) {
                    output = output + String.fromCharCode(chr2);
                }
                if (enc4 != 64) {
                    output = output + String.fromCharCode(chr3);
                }
            }
            output = js.lang.String.decodeFromUTF8(output);
            return output;
        },

        format: function (format, params) {

        },

        indexOf: function (s, ch) {
            return s.toString().indexOf(ch);
        },

        isEmpty: function (s) {
            if (s.length == 0)
                return true;
            else
                return false;
        },

        isNullOrEmpty: function (str) {
            return !!(str === null || str === '');
        },

        isNullOrWhiteSpace: function (str) {
            return !!(str === null || str === " ");
        },

        lastIndexOf: function () {

        },

        length: function (s) {
            return s.toString().length;
        },

        matches: function (x) {
            return s.toString().match(x);
        },

        // Remove last characters from string s with length of c.
        removeLast: function (s, c) {
            return s.slice(0, s.length - c);
        },

        replace: function (s, sub) {
            var str;
            if (typeof s !== "string") {
                str = s.toString();
            } else {
                str = s;
            }

            return str.replace(s, sub);
        },

        replaceAll: function (s, sub, mode, ignoreCase) {
            if (ignoreCase == null || typeof ignoreCase === "undefined") {
                ignoreCase = false;
            }

            if (mode == "regex") {
                var target = this;
                if (ignoreCase == false) {
                    return target.replace(new RegExp(s, 'g'));
                } else {
                    return target.replace(new RegExp(s, 'gi'));
                }
            }
            else {
                var target = this;
                return target.split(s).join(sub);
            }
        },

        startsWith: function (s, pref) {
            return s.toString().startsWith(pref);
        },

        // Convert string s to Base64 format.
        toBase64String: function (s) {
            var output = "";
            var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
            var i = 0;

            input = js.lang.String.encodeToUTF8(s);
            while (i < input.length) {
                chr1 = input.charCodeAt(i++);
                chr2 = input.charCodeAt(i++);
                chr3 = input.charCodeAt(i++);

                enc1 = chr1 >> 2;
                enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
                enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
                enc4 = chr3 & 63;

                if (isNaN(chr2)) {
                    enc3 = enc4 = 64;
                } else if (isNaN(chr3)) {
                    enc4 = 64;
                }
                output = output +
                base64.charAt(enc1) + base64.charAt(enc2) +
                base64.charAt(enc3) + base64.charAt(enc4);
            }
            return output;
        },

        toBinaryString: function (input) {
            var bin = "";
            var output = "";
            if (typeof input !== "string") { bin = input.toString() }
            else { bin = input };
            for (var i = 0; i < bin.length; i++) {
                output += bin[i].charCodeAt(0).toString(2) + " ";
            }
            return output;
        },

        toCharArray: function (str) {
            var len = str.length;
            var arr = new Array(len);
            for (var i = 0; i < len; i++) {
                arr.push(str.substring(str, i));
            }
            return arr;
        },

        // Convert a number to hexadecimal string.
        toHexString: function (number, padding, ucase) {
            var hex = Number(number).toString(16);
            padding == typeof (padding) === "undefined" || padding === null ? padding = 2 : padding;
            while (hex.length < padding) {
                hex = "0" + hex;
            }

            hex = "0x" + hex;

            // uppercase/lowercase format
            if (ucase != null && typeof (ucase) === 'boolean' && ucase == true) {
                return hex.toUpperCase();
            }
            else {
                return hex.toLowerCase();
            }
        },

        toLower: function (input) {
            if (typeof input !== "string") { return input };
            var lc = input.toLowerCase();
            if ('i' !== 'I'.toLowerCase()) {
                input.replace(/[A-Z]/g, function (ch) { return String.fromCharCode(ch.charCodeAt(0) | 32); });
                return input;
            } else {
                return lc;
            }
        },

        toOctalString: function (input) {
            return input.toString(8);
        },

        toUpper: function (input) {
            if (typeof input !== "string") { return input };
            var uc = input.toUpperCase();
            if ('I' !== 'i'.toUpperCase()) {
                input.replace(/[a-z]/g, function (ch) { return String.fromCharCode(ch.charCodeAt(0) & ~32); });
                return input;
            } else {
                return uc;
            }
        }
    };

    /*
     *  Language class libraries.
     */
    js.lang.Exception = {
        IllegalArgumentException: new Error("The method argument is not supported."),
        IndexOutOfBoundsException: new Error("Index array is out of bounds from defined length."),
        MalformedURLException: new Error("The provided URL does not match with standard URL format."),
        NegativeArraySizeException: new Error("Array must declared in positive number of size."),
        NoSuchFieldException: new Error("The required field is not found on this context."),

        // Derived from NoSuchMethodException
        NoSuchFunctionException: new Error("The required function is not found on this context."),

        // Derived from NullPointerException
        NullValueException: new Error("Value cannot be null."),
        NumberFormatException: new Error("The number format is invalid for this context."),
        OutOfRangeException: new Error("The given number is out of range for corresponding data type."),
        RuntimeException: new Error("Runtime error occurred. Source: " + js.lang.Throwable.printStackTrace),
        UnsupportedOperationException: new Error("The operation is not supported on this version."),
        UndefinedException: new Error("The variable you've trying to assign is undefined."),

        equals: function (a, b) {
            return js.lang.Object.equals(a, b);
        },

        referenceEquals: function (a, b) {
            return js.lang.Object.referenceEquals(a, b);
        },

        printStackTrace: function () {
            return js.lang.Throwable.printStackTrace();
        }
    };

    js.lang.System = {
        arrayCopy: function (arr) {
            var newarr = arr.slice();
            return newarr;
        },


    };

    js.lang.System.in = {
        read: function (element) {
            if ($ || jQuery) {
                return $(element).val();
            }
            else {
                return document.getElementById(name).value;
            }
        },

        readLine: function (element) {

        }
    };

    js.lang.System.out = {
        printf: function (element, input) {
            return document.getElementById(element).innerHTML = value;
        },

        printHtml: function (element, input) {
            return document.getElementById(element).firstChild.nodeValue = input;
        }
    };

    js.lang.System.err = {

    };

    // 

    /*
     *  Input/output interface methods & properties.
     *  Package: js.io
     */

    js.io.FileUpload = {

    };

    js.io.FileReader = {

    };

    js.io.FileWriter = {

    };

    js.io.ObjectInputStream = {

    };

    js.io.ObjectOutputStream = {

    };

    /*
     *  Network interface methods & properties.
     *  Package: js.net
     */

    js.net.Json = {
        getJson: function (url, data, result) {
            if (jQuery || $) {
                return $.getJSON(url, data, result);
            }
        },

        parseJson: function (json) {
            if (jQuery || $) {
                return JSON.parse(json);
            }
        },

        toJson: function (val, replacer, space) {
            return JSON.stringify(val, replacer, space);
        },
    };

    js.net.Manifest = {
        createManifest: function () {

        },

        readManifest: function () {

        },

        removeManifest: function () {

        }
    };

    js.net.URL = {
        getURL: function () {

        },

        setURL: function (url) {

        },
    };

    /*
     *  NIO interface methods & properties.
     *  Package: js.nio
     */

    js.nio.ByteBuffer = {

    };

    js.nio.ByteOrder = {

    };

    js.nio.BufferOverflowException = new Error("Buffer overflow.");
    js.nio.BufferUnderflowException = new Error("Buffer underflow.");
    js.nio.InvalidMarkException = new Error("");
    js.nio.ReadOnlyBufferException = new Error("The selected buffer is read-only.");

    /*
     *  Input/output interface methods & properties.
     *  Package: js.util
     */

    js.util.ArrayList = {
        add: function (list, value) {
            list.push(value);
        },

        remove: function (list) {
            list.pop();
        }
    };

    js.util.Date = {

        /*
         *   Date class throws 2 exceptions:
         *   - IllegalArgumentException
         *   - NullValueException
         */

        computeFields: function (millis) {
            var tm = new Date();
            tm.setDate(millis);
        },

        // Compute date value to milliseconds.
        computeTime: function (field) {
            var tm = new Date(field);
            tm.getMilliseconds();
        },

        defaultFlags: function () {
            return {
                empty: false,
                unusedTokens: [],
                unusedInput: [],
                overflow: -2
            };
        },

        equals: function (a, b) {
            return !!(a === b);
        },

        getCurrentDate: function () {
            return new Date().getDate();
        },

        getCurrentTime: function () {
            return new Date().getTime();
        },

        getCurrentTimeMillis: function () {
            return new Date().getMilliseconds();
        },

        getFlags: function (input) {
            if (input.pf == null) {
                input.pf = js.util.Date.defaultFlags();
            }
            return input.pf;
        },

        getHour: function () {
            return new Date().getHours();
        },

        getMinute: function () {
            return new Date().getMinutes();
        },

        getMonthNum: function () {
            return new Date().getMonth();
        },

        getSecond: function () {
            return new Date().getSeconds();
        },

        getUTCDateFormat: function () {
            return new Date().getUTCDate();
        },

        getUTCDays: function () {
            return new Date().getUTCDay();
        },

        getUTCMonths: function () {
            return new Date().getUTCMonth();
        },

        // @Override
        // Get UTC-based 4 digit year.
        getUTCYear: function () {
            return new Date().getUTCFullYear();
        },

        getUTCTime: function () {
            var dt = new Date();
            return dt.getUTCHours() + ":" + dt.getUTCMinutes + ":" + dt.getUTCSeconds() + "Z";
        },

        getTimeZone: function () {
            return new Date().getTimezoneOffset();
        },

        // @Override
        // Override native getYear() function for 4 digits.
        getYear: function () {
            return new Date().getFullYear();
        },

        isDate: function (input) {
            return input instanceof Date || Object.prototype.toString.call(input) === '[object Date]';
        },

        setCurrentDate: function (dt) {
            return new Date().setDate(dt);
        },

        setCurrentMonth: function (mon, dt) {
            return new Date().setMonth(mon, dt);
        },

        setYear: function (yr, mon, dt) {
            return new Date().setFullYear(yr, mon, dt);
        },

        setHour: function (hr, min, sec, ms) {
            if (typeof ms === "undefined")
                ms = 0;
            if (typeof sec === "undefined")
                sec = 0;
            if (typeof min === "undefined")
                min = 0;
            return new Date().setHours(hr, min, sec, ms);
        },

        setMinute: function (min, sec, ms) {
            if (typeof ms === "undefined")
                ms = 0;
            if (typeof sec === "undefined")
                sec = 0;
            return new Date().setMinutes(min, sec, ms);
        },

        setSecond: function (sec, ms) {
            if (typeof ms === "undefined")
                ms = 0;
            return new Date().setSeconds(sec, ms);
        },

        setCurrentTimeMillis: function (ms) {
            return new Date().setMilliseconds(ms);
        }
        

    };

    js.util.Math = {
        /* Constants */
        // Natural logarithm
        E: 2.718281828459045,

        // Pi
        PI: 3.141592653589793,

        addExact: function () {

        },

        asin: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        acos: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        atan: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        cbrt: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        ceil: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        cos: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return Math.cos(x);
        },

        cosec: function () {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return (1 / Math.sin(x));
        },

        // Hyperbolic cosine (from Java).
        cosh: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return (Math.pow(Math.exp, x) + Math.pow(Math.exp, x * -1)) / 2;
        },

        cot: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return (Math.cos(x) / Math.sin(x));
        },

        // Identity matrix
        createIdentity: function (order) {
            if (order === 2) {
                var mt = [[1, 0], [0, 1]];

            } else if (order === 3) {
                var mt = [[1, 0, 0], [0, 1, 0], [0, 0, 1]];
            } else
                throw new Error("Number out of order");
        },

        dcos: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            var rad = x * Math.PI / 180;

            return Math.cos(rad);
        },

        dtan: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            var rad = x * Math.PI / 180;

            return Math.tan(rad);
        },

        decrementExact: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }
        },

        dsin: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            var rad = x * Math.PI / 180;

            return Math.sin(rad);
        },

        // Exponential expm1 (from Java).
        expm1: function (x) {
            return Math.pow(Math.exp, 1) - 1;
        },

        floor: function (x) {
            return Math.floor(x);
        },

        grad: function (x, y) {
            return (y / x);
        },

        // Hypothenuse method (from Java).
        hypot: function (x, y) {
            return Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2));
        },

        incrementExact: function () {

        },

        ln: function (x) {
            return Math.log(x);
        },

        log: function (x) {

        },

        max: function (a, b) {
            if (a > b) {
                return a;
            } else {
                return b;
            }
        },

        min: function (a, b) {
            if (a < b) {
                return a;
            } else {
                return b;
            }
        },

        multipleExact: function () {

        },

        negateExact: function () {

        },

        rint: function () {

        },

        rand: function () {
            return Math.random();
        },

        round: function (input) {
            return Math.round(input);
        },

        sin: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return Math.sin(x);
        },

        // Hyperbolic sine (from Java).
        sinh: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return (Math.pow(Math.exp, x) - Math.pow(Math.exp, x * -1)) / 2;
        },

        sec: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return (1 / Math.cos(x));
        },

        sqrt: function (x) {
            return Math.sqrt(x);
        },

        subtractExact: function () {

        },

        tan: function (x) {
            return Math.tan(x);
        },

        // Hyperbolic tangent (from Java).
        tanh: function (x) {
            if (x == Infinity || isNaN(x)) {
                return NaN;
            }

            return js.util.Math.sinh(x) / js.util.Math.cosh(x);
        },

        toDegrees: function (rad) {
            return (rad * js.util.Math.PI / 180);
        },

        toRadians: function (deg) {

        },

        /* Exceptions */

        // Divide by zero exception
        ArithmeticExecption: new Error("Arithmetic error in math operation."),
        DivideByZeroException: new Error("Division by zero."),
        InputMismatchException: new Error("Input number mismatch."),
    };

    // DOM objects
    

    // Primitive type declarations
    var boolean = js.lang.Boolean;
    var byte = js.lang.Byte;
    var char = js.lang.Character;
    var double = js.lang.Double;
    var float = js.lang.Float;
    var int = js.lang.Integer;
    var long = js.lang.Long;
    var short = js.lang.Short;
    var string = js.lang.String;

    window.js = js;
    window.boolean = boolean;
    window.byte = byte;
    window.char = char;
    window.double = double;
    window.float = float;
    window.int = int;
    window.long = long;
    window.short = short;
    window.string = string;

    return js;
}));
// }