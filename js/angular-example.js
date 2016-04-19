angular.module('myModule', []);
angular.module('myModule').factory(objectChangeFactory);
angular.module('myModule').factory(authHttpResponseInterceptor);
angular.module('myModule').run(configFunction);
angular.module('myModule').config(configFunction);

objectChangeFactory.$inject = ['$http'];
authHttpResponseInterceptor.$inject = ['$q', '$rootScope'];
runFunction.$inject = ['$rootScope', '$http'];
configFunction.$inject = ['$httpProvider'];

function objectChangeFactory($http) {
	function setValues(assemblyhash, objecthash, getdatetime, forcesave)
	{
	    $http.defaults.headers.common["assemblyhash"] = assemblyhash;
	    $http.defaults.headers.common["objecthash"] = objecthash;
	    $http.defaults.headers.common["getdatetime"] = getdatetime;

	    if(forcesave !== undefined)
	        $http.defaults.headers.common["forcesave"] = forcesave;
	}

	return {
	    setValues: setValues
	};
}

function authHttpResponseInterceptor($q, $rootScope) {
	return {
	    request: function(request)
	    {
	        return request;
	    },
	    response: function(response) {

	        if(response.config.method == "GET" && response.headers().assemblyhash !== undefined
	            && response.headers().objecthash !== undefined && response.headers().getdatetime !== undefined) {
	            $rootScope.setObjectValues(response.headers().assemblyhash, response.headers().objecthash, response.headers().getdatetime)
	        }

	        return response || $q.when(response);
	    },
	    responseError: function(rejection) {
	        if(rejection.status == 409) {
	            console.error('Conflict in the object you tried to save, please reload the page or force a save....');
	        }

	        return $q.reject(rejection);
	    }
	};
}

function runFunction($rootScope, $http) {
	$rootScope.setObjectValues = setObjectValues;

	function setObjectValues(assemblyhash, objecthash, getdatetime, forcesave) {
        objectChange.setValues(assemblyhash, objecthash, getdatetime, forcesave);
    }
}

function configFunction($httpProvider) {
	$httpProvider.interceptors.push('authHttpResponseInterceptor');
}