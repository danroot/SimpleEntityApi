if (angular) {
    angular.module('serverValidate', [])
        .directive('serverValidate', [function() {
            console.log('serverValidate: wiring up');

            return {
                require: 'ngModel',
                link: function(scope, ele, attrs, c) {
                    console.log('serverValidate: link');
                    console.log(scope);
                    console.log(ele);
                    console.log(attrs);
                    console.log(c);
                    var attributeText = attrs.serverValidate;
                    var attributeParts = attributeText == null ? [] : attributeText.split(',');

                    var modelStateExpression = attributeParts[0];
                    if (modelStateExpression.length > 0) modelStateExpression = modelStateExpression + '.';
                    modelStateExpression = modelStateExpression + 'modelState';

                    var modelStateKey = attributeParts.length > 1 ? attributeParts[1] : attrs.ngModel.replace(attrs.serverValidate + '.', '');
                    modelStateKey = modelStateKey.replace('$index', scope.$index);
                    console.log('Will watch ' + modelStateExpression + ' for ' + modelStateKey);
                    scope.$watch(modelStateExpression, function() {
                        console.log('serverValidate: modelState changed');
                        var modelState = scope.$eval(modelStateExpression);

                        console.log('serverValidate: setting error for ' + modelStateKey);
                        if (modelState == null) return;
                        if (modelState[modelStateKey]) {
                            c.$setValidity('server', false);
                            c.$error.server = modelState[modelStateKey];
                        } else {
                            c.$setValidity('server', true);
                            c.$error.server = [];
                        }
                    });
                    scope.$watch(attrs.ngModel, function(oldValue, newValue) {
                        if (oldValue != newValue) {

                            c.$setValidity('server', true);
                            c.$error.server = [];
                        }
                    });
                }
            };
        }]);

    function isScrolledIntoView(elem) {
        var docViewTop = $(window).scrollTop();
        var docViewBottom = docViewTop + $(window).height();

        var elemTop = $(elem).offset().top;
        var elemBottom = elemTop + $(elem).height();
        console.log({ docViewTop: docViewTop, docViewBottom: docViewBottom, elemTop: elemTop, elemBottom: elemBottom });
        if (elemTop == 0) return false;
        return elemTop >= docViewTop && elemTop <= docViewBottom;
    }


    var bootstrapGrid = angular
        .module('bootstrapGrid', [])
        .directive('bootstrapGrid', ['$filter', '$templateCache', function($filter, $templateCache) {
            addBootstrapGridTemplates($templateCache);

            return {
                scope: true,
                templateUrl: 'bootstrapGrid.html', //TODO: get url somehow or include
                compile: function() {
                    return {
                        pre: function($scope, iElement, iAttrs) {
                            $scope.filterOrDefault = function(col, item) {
                                var result = item[col.field];
                                if (col.cellFilter == null || col.cellFilter == '') return result;
                                var filterParts = col.cellFilter.split(':');
                                var filter = filterParts[0];
                                var expression = filterParts.length > 1 ? filterParts[1] : null;
                                var comparator = filterParts.length > 2 ? filterParts[2] : null;
                                return $filter(filter)(result, expression, comparator);
                            };
                            $scope.grid = $scope.$parent[iAttrs.bootstrapGrid];
                            $scope.$watch('grid.pagingOptions', function(newVal, oldVal) {
                                if (newVal !== oldVal && (newVal.currentPage !== oldVal.currentPage || newVal.pageSize !== oldVal.pageSize)) {
                                    $scope.grid.loadIn(100, $scope.grid.options.pagingMode == 'append');
                                }
                            }, true);
                            $scope.$watch('grid.filterOptions.filterText', function(newVal, oldVal) {
                                if (newVal !== oldVal) {
                                    $scope.grid.pagingOptions.currentPage = 1;
                                    $scope.grid.loadIn(300);
                                }
                            }, true);
                            $scope.$watch('grid.loading', function() {
                                //console.log('hi');
                                //$('.cmdLoadMore').each(function (i, e) {
                                //    console.log(isScrolledIntoView(e));
                                //    if (isScrolledIntoView(e)) {
                                //        console.log($scope.$eval($(e).attr("data-ng-click")));
                                //        //$(e).click();
                                //    }
                                //});
                            });
                        }
                    };
                },
            };
        }]);

}