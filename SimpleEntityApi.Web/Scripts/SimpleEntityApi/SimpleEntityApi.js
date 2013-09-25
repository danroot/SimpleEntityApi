///#source 1 1 /Scripts/SimpleEntityApi/SimpleEntityApi.core.js


function SimpleApiListSource(apiUrl, $http, options) {
    var self = this;

    this.options = {
        allowPaging: options.allowPaging || false,
        pagingMode: options.pagingMode || 'default',
        allowSorting: options.allowSorting || false,
        allowSearching: options.allowSearching || false,
        allowDelete: options.allowDelete || false,
        enableMetadataLookup: options.enableMetadataLookup || false,
        topButtonsTemplate: options.topButtonsTemplate || '',
        rowButtonsTemplate: options.rowButtonsTemplate || 'rowButtonsTemplate.html',
        pagingTemplate: options.pagingMode == 'append' ? 'appendPagingTemplate.html' : 'defaultPagingTemplate.html'
    };
    this.columnDefs = options.columnDefs;
    this.rowClick = options.rowClick || function () { };
    this.newItem = {};
    this.items = [];
    this.pagingOptions = {
        pageSizes: [10, 50, 100],
        pageSize: options.itemsPerPage || 10,
        currentPage: 1,
        pageCount: -1,
        pagerPages: [1]
    };
    this.itemCount = 0;
    this.sort = options.sort || {};
    this.lastQuery = '';
    this.filterOptions = { filterText: '', useExternalFilter: true };
    this.loading = false;
    this.metaData = [];

    this.forNgGrid = function ($scope, gridName) {
        self.data = gridName + '.items';
        self.totalServerItems = gridName + '.itemCount',
        self.showFooter = true;
        self.enablePaging = true;

        $scope.$watch(gridName + '.pagingOptions', function (newVal, oldVal) {
            if (newVal !== oldVal && (newVal.currentPage !== oldVal.currentPage || newVal.pageSize !== oldVal.pageSize)) {
                self.loadIn(100);
            }
        }, true);
        $scope.$watch(gridName + '.filterOptions.filterText', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                self.pagingOptions.currentPage = 1;
                self.loadIn(300);
            }
        }, true);

        return self;
    };
    this.applyMetadata = function () {
        for (var i = 0; i < self.metaData.length; i++) {
            var columnMetadata = self.metaData[i];
            for (var c = 0; c < self.columnDefs.length; c++) {
                var col = self.columnDefs[c];
                if (col.field == columnMetadata.Name) {
                    col.metaData = columnMetadata;
                    if (col.dataType == null) col.dataType = columnMetadata.TypeUsageName.toLowerCase();
                    if (col.displayName == null && columnMetadata.DisplayName != null) col.displayName = columnMetadata.DisplayName;

                    //These are not metadata-specific and may be better elsewhere.
                    if (col.displayName == null) col.displayName = col.field;
                    //These are angular-specific and may be better elsewhere.
                    if (col.cellFilter == null && col.dataType == 'double') col.cellFilter = 'number:2';
                    if (col.cellFilter == null && col.dataType == 'datetime') col.cellFilter = 'date:shortDate';
                }
            }
        }
    };
    this.loadMetadata = function () {
        return $http
            .get(apiUrl + '/meta')
            .success(function (data) {
                self.metaData = data;
                self.applyMetadata();
            });
    };
    this.loadTimer = null;
    this.loadIn = function (ms, append) {
        self.loading = true;
        window.clearTimeout(this.loadTimer);
        self.loadTimer = window.setTimeout(function () {
            self.load(append);
        }, ms);
    };
    this.TryParseInt = function (str, defaultValue) {
        var retValue = defaultValue;
        if (str != null) {
            if (str.length > 0) {
                str = str.replace(',', '');
                if (!isNaN(str)) {
                    retValue = parseInt(str);
                }
            }
        }
        return retValue;
    };
    this.TryParseFloat = function (str, defaultValue) {
        var retValue = defaultValue;
        if (str != null) {
            if (str.length > 0) {
                str = str.replace(',', '');
                if (!isNaN(str)) {
                    retValue = parseFloat(str);
                }
            }
        }
        return retValue;
    };
    this.TryParseDate = function (str, defaultValue) {
        var retValue = defaultValue;
        if (str != null) {
            if (str.length > 0) {
                var val = Date.parse(str);
                if (!isNaN(val)) {
                    retValue = new Date(val);
                }
            }
        }
        return retValue;
    };
    this.getTypedFilter = function (filterText) {
        if (filterText == null || filterText == '') return {};
        return {
            asDate: self.TryParseDate(filterText),
            asInt: self.TryParseInt(filterText),
            asFloat: self.TryParseFloat(filterText),
            asText: filterText
        };
    };
    this.getSearchODataFilter = options.getSearchODataFilter || function (filterText) {
        var typedFilter = self.getTypedFilter(filterText);
        var result = [];
        for (var i = 0; i < self.columnDefs.length; i++) {
            var col = self.columnDefs[i];
            if (typedFilter.asDate && col.dataType == 'datetime') {
                var offset = (new Date().getTimezoneOffset() * 60 * 1000);
                var start = new Date(Date.parse(filterText + ' 0:00') + offset);
                var end = new Date(Date.parse(filterText + ' 23:00') + offset);
                result.push(col.field + ' gt datetime\'' + start.toISOString() + '\' and ' + col.field + ' lt datetime\'' + end.toISOString() + '\'');

            }
            if (typedFilter.asFloat && col.dataType == 'double') result.push('floor(' + col.field + ') eq floor(' + typedFilter.asFloat.toFixed(2) + ') or ceiling(' + col.field + ') eq ceiling(' + typedFilter.asFloat.toFixed(2) + ')');
            if (typedFilter.asFloat && col.dataType == 'int32') result.push(col.field + ' eq ' + typedFilter.asFloat.toFixed(0));
            if (typedFilter.asText && col.dataType == 'string') result.push('substringof(\'' + typedFilter.asText + '\',' + col.field + ') eq true');
        }
        return result.join(' or ');
    };
    this.toggleSort = function (propertyName) {

        var current = self.sort[propertyName];
        if (current == null)
            self.sort[propertyName] = '';
        else if (current == '')
            self.sort[propertyName] = 'desc';
        else if (current == 'desc')
            delete self.sort[propertyName];

        self.load();
    };
    this.getSortOData = function () {
        var result = [];
        for (var prop in this.sort) {
            result.push(prop + ' ' + this.sort[prop]);
        }
        var resultString = result.join(',');
        return resultString;
    };
    this.loadNewItem = function () {
        $http.get(apiUrl + '/new')
            .success(function (data) {
                self.newItem = data;
            });
    };
    this.addMany = function (items) {
        return $http.post(apiUrl + '/many', items)
            .success(function () {
                self.load();
            });
    };
    this.add = function (item) {
        var itemToAdd = item || self.newItem;

        return $http.post(apiUrl, itemToAdd)
            .success(function () {
                if (item == null) self.loadNewItem();
                self.load();
            });
    };
    this.update = function (item) {
        return $http.put(apiUrl, item)
            .success(function () {

            });
    };
    this.delete = function (item) {
        $http.delete(apiUrl, { data: item })
            .success(function () {
                var i = self.items.indexOf(item);
                if (i != -1) {
                    self.items.splice(i, 1);
                }
            });
    };
    this.confirmDelete = function (message, item) {
        if (confirm(message)) {
            self.delete(item);
        }
    };
    this.goToPage = function (page) {
        self.pagingOptions.currentPage = page;

    };
    this.firstPage = function () {
        self.pagingOptions.currentPage = 1;

    };
    this.nextPage = function () {
        if (self.pagingOptions.currentPage < self.pagingOptions.pageCount) {
            self.pagingOptions.currentPage = self.pagingOptions.currentPage + 1;

        }
    };
    this.previousPage = function () {

        if (self.pagingOptions.currentPage > 1) {
            self.pagingOptions.currentPage = self.pagingOptions.currentPage - 1;

        }
    };
    this.lastPage = function () {

        self.pagingOptions.currentPage = self.pagingOptions.pageCount;

    };
    this.getPageInterval = function (currentPage, pagesToShow, pageCount) {
        var halfPagesToShow = pagesToShow / 2;
        return {
            start: Math.ceil(currentPage > halfPagesToShow ? Math.max(Math.min(currentPage - halfPagesToShow, (pageCount - pagesToShow)), 1) : 1),
            end: Math.ceil(currentPage > halfPagesToShow ? Math.min(currentPage + halfPagesToShow - 1, pageCount) : Math.min(pagesToShow, pageCount))
        };
    };

    this.load = function (append) {
        self.loading = true;
        var urlParts = [];
        if (options.allowPaging) {
            urlParts.push('$inlinecount=allpages');
            urlParts.push('$skip=' + ((self.pagingOptions.currentPage - 1) * self.pagingOptions.pageSize));
            urlParts.push('$top=' + self.pagingOptions.pageSize);
        }
        if (options.allowSorting) {
            var sortOData = self.getSortOData();
            if (sortOData != null && sortOData != '') urlParts.push('$orderby=' + sortOData);
        }
        var filterParts = [];
        if (options.allowSearching) {
            var searchODataFilter = self.getSearchODataFilter(self.filterOptions.filterText);
            if (searchODataFilter != null && searchODataFilter != '') filterParts.push('(' + searchODataFilter + ')');
        }
        if (filterParts.length > 0) {
            var filter = '$filter=' + filterParts.join(' and ');
            urlParts.push(filter);
        }
        var queryUrl = apiUrl + "/?" + urlParts.join('&');

        self.lastQuery = queryUrl;

        return $http.get(queryUrl)
            .then(function (result) {
                var data = result.data;
                if (append == true) {
                    for (var i = 0; i < data.Results.length; i++) {
                        var resultItem = data.Results[i];
                        self.items.push(resultItem);
                    }
                } else {
                    self.items = data.Results;
                }
                self.itemCount = data.Count;
                if (self.options.allowPaging) {
                    self.pagingOptions.pageCount = Math.ceil(self.itemCount / self.pagingOptions.pageSize);
                    self.pagingOptions.pagerPages = [];
                    var interval = self.getPageInterval(self.pagingOptions.currentPage, 10, self.pagingOptions.pageCount);
                    for (var i = interval.start; i <= interval.end; i++) {
                        self.pagingOptions.pagerPages.push(i);
                    }
                }
                self.loading = false;

            }, function (e) {
                self.loading = false;

            });
    };
    this.init = function () {
        $http.defaults.headers.delete = { 'Content-Type': 'application/json' };
        $http.defaults.headers.get = { 'Content-Type': 'application/json' };

        if (self.options.enableMetadataLookup) {
            self.loadMetadata().then(self.load);
        } else {
            self.load();
        }
        self.loadNewItem();

    };



    this.init();
}

function SimpleApiFormSource(apiUrl, $http, id) {
    var self = this;

    this.modelState = {};
    this.item = {};
    this.loading = false;
    this.saving = false;
    this.isNew = false;
    this.load = function () {

        if (id != null) {
            self.getById();
            self.save = self.update;
            self.isNew = false;
        } else {
            self.getNew();
            self.save = self.add;
            self.isNew = true;
        }
    };
    this.getById = function () {

        self.loading = true;
        return $http.get(apiUrl + '/' + id) 
            .success(function (data) {
                self.item = data;
                self.loading = false;
            })
            .error(function () {
                self.loading = false;
            });
    };
    this.getNew = function () {
        $http.get(apiUrl + '/new')
            .success(function (data) {
                self.item = data;
            });
    };
    this.update = function () {
        self.saving = true;
        return $http.put(apiUrl, self.item)
            .success(function (data) {
                self.modelState = {};
                self.item = data;
                self.saving = false;
                if (self.onSave) self.onSave();
            }).error(function (data) {
                self.modelState = data.ModelState;
                self.saving = false;
            });
    };
    this.add = function () {
        self.saving = true;
        return $http.post(apiUrl, self.item)
            .success(function (data) {
                self.modelState = {};
                self.item = data;
                self.saving = false;
                if (self.onSave) self.onSave();
            }).error(function (data) {
                self.modelState = data.ModelState;
                self.saving = false;
            });
    };
    this.load();
}


