///#source 1 1 /Scripts/SimpleEntityApi/SimpleEntityApi.angular.templates.js

//Templates
function addBootstrapGridTemplates($templateCache){
 $templateCache.put('appendPagingTemplate.html','<div class="col-xs-12" style="text-align: center">    <span data-ng-show="grid.loading">Loading...</span>    <a class="cmdLoadMore" data-ng-hide="grid.loading" data-ng-click="grid.nextPage()" style="cursor: pointer">Load more</a></div>');
  $templateCache.put('bootstrapGrid.html','<div class="row">    <div class="col-xs-4 col-sm-8">        <span data-ng-include="grid.options.topButtonsTemplate"></span>        <span data-ng-show="grid.loading">Loading...</span>    </div>    <div class="col-xs-8 col-sm-4">        <div class="input-group">            <input type="text" class="form-control" placeholder="Search" data-ng-model="grid.filterOptions.filterText" />            <span class="input-group-btn" data-ng-click="grid.load()">                <button class="btn btn-default">                    <span class="glyphicon glyphicon-search"></span>                </button>            </span>        </div>    </div></div><table class="table table-hover">    <thead>        <tr>            <th style="width: 50px;"></th>            <th class="pointer" data-ng-repeat="col in grid.columnDefs" data-ng-click="grid.toggleSort(col.field)">{{col.displayName}}                 <span class="glyphicon" data-ng-class="{\'glyphicon-chevron-up\':grid.sort[col.field] == \'\', \'glyphicon-chevron-down\':grid.sort[col.field] == \'desc\'}"></span>            </th>        </tr>    </thead>    <tbody>        <tr data-ng-repeat="item in grid.items">            <td data-ng-include="grid.options.rowButtonsTemplate"></td>            <td class="pointer" data-ng-repeat="col in grid.columnDefs" data-ng-click="grid.rowClick(item)">{{filterOrDefault(col,item)}}</td>        </tr>    </tbody></table><div class="row" data-ng-include="grid.options.pagingTemplate"></div>');
  $templateCache.put('defaultPagingTemplate.html','<div class="col-xs-2">    <select class="form-control" data-ng-options="size for size in grid.pagingOptions.pageSizes" data-ng-model="grid.pagingOptions.pageSize"></select></div><div class="col-xs-10">    <ul class="pagination" style="margin: 0;">        <li><a class="pointer" data-ng-click="grid.firstPage()">&lt;&lt;</a></li>        <li><a class="pointer" data-ng-click="grid.previousPage()">&lt;</a></li>        <li data-ng-repeat="page in grid.pagingOptions.pagerPages" data-ng-class="{active:page==grid.pagingOptions.currentPage}">            <a class="pointer" data-ng-click="grid.goToPage(page)">{{page}}</a>        </li>        <li><a class="pointer" data-ng-click="grid.nextPage()">&gt;</a></li>        <li><a class="pointer" data-ng-click="grid.lastPage()">&gt;&gt;</a></li>    </ul></div>');
  $templateCache.put('rowButtonsTemplate.html',' <span class="glyphicon glyphicon-remove pointer"        data-ng-show="grid.options.allowDelete"        data-ng-click="grid.confirmDelete(\'This cannot be undone.  Are you sure?\',item)"></span>');
 }

///#source 1 1 /Scripts/SimpleEntityApi/SimpleEntityApi.angular.module.js
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
