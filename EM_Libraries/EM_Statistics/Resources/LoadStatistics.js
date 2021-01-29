// This library fills the statistics UI with results, using the JSON obtained from the EM_LightBackEnd server.
// HTML building blocks

var CONTAINER_HEAD_ID_PREFIX = "ContainerHead_";
var CONTAINER_BODY_ID_PREFIX = "ContainerBody_";
var SERVER_UPDATE_MILLISECONDS = 200;

var firstLoad = true;
var currentTab = 0;
var changingPackage = false;
var serverRecallTimeout = null;

// This calls the BackEnd to get the progress update and runs HandleProgress once the response is back
function TryGetStats(key) {
    var param = key==""?"":"packageKey="+key;
    embe_BackEndRequest(emsta_MakeStatisticsUI, responderKey + "_LoadStatistics", param);
}

function ShowStatsContent(key) {
    // first clear any timeouts (as they will be re-intruduced below if still needed) and hide the inner "loading"
    if (serverRecallTimeout) clearTimeout(serverRecallTimeout);
    document.getElementById("emsta-package-page-contents-loader").style.display = "none";

    // if headers exist, show the statistics header
    if (document.getElementById("emsta-statistics-title-" + key)) {
        var emstaTitles = document.getElementsByClassName("emsta-statistics-title");
        for (i = 0; i < emstaTitles.length; i++) emstaTitles[i].style.display = "none";
        document.getElementById("emsta-statistics-title-" + key).style.display = "block";

        var emstaSubTitles = document.getElementsByClassName("emsta-statistics-subtitle");
        for (i = 0; i < emstaSubTitles.length; i++) emstaSubTitles[i].style.display = "none";
        document.getElementById("emsta-statistics-subtitle-" + key).style.display = "block";

        var emstaTabs = document.getElementsByClassName("emsta-page-tabContainer");
        for (i = 0; i < emstaTabs.length; i++) emstaTabs[i].style.display = "none";
        if (document.getElementById("emsta-page-tabContainer-" + key)) document.getElementById("emsta-page-tabContainer-" + key).style.display = "block";

        var emstaContents = document.getElementsByClassName("emsta-package-page-contents");
        for (i = 0; i < emstaContents.length; i++) emstaContents[i].style.display = "none";
        document.getElementById("emsta-package-page-contents-" + key).style.display = "block";

        // export buttons loaded
        var emstaExport = document.getElementsByClassName("emsta-export");
        if (emstaExport && emstaExport.length > 0) {
            for (i = 0; i < emstaExport.length; i++) emstaExport[i].style.display = "none";
            var exp = document.getElementById("emsta-export-" + key);
            if (exp) exp.style.display = "inline-block";
        }

        // if showing package buttons
        var emstaPackageButtons = document.getElementsByClassName("emsta-package-button");
        if (emstaPackageButtons && emstaPackageButtons.length > 0) {
            for (i = 0; i < emstaPackageButtons.length; i++) emstaPackageButtons[i].className = emstaPackageButtons[i].className.replace(" w3-grey w3-hover-grey", "");
            document.getElementById("emsta-package-button-" + key).className += " w3-grey w3-hover-grey";
        }
    }

    // if content is not yet loaded, try to load it again
    if (document.getElementById("emsta-package-page-contents-" + key).getAttribute("loaded") == "false") {
        var emstaContents = document.getElementsByClassName("emsta-package-page-contents");
        for (i = 0; i < emstaContents.length; i++)  emstaContents[i].style.display = "none";
        document.getElementById("emsta-package-page-contents-loader").style.display = "block";
        serverRecallTimeout = setTimeout(function () { TryGetStats(key); }, SERVER_UPDATE_MILLISECONDS);
    }
    else emsta_TabButton_Click(currentTab); // else switch to the right tab

    resizeStatisticsWindow();   // make sure that the tab content always has the right size 

    return;
}


function emsta_MakeStatisticsUI(statistics) {
    var key = statistics.packageKey;
    if (document.getElementById("emsta-package-page-contents-" + key).getAttribute("loaded") == "true") {
        ShowStatsContent(key);
        return;
    }

    // if haven't done so yet, build the headers 
    if (!document.getElementById("emsta-statistics-title-" + key))
        emsta_MakeStatisticsHeader(statistics.results.info, key);

    // if the content is still not ready, show only what you have so far
    if (!statistics.completed) {
        ShowStatsContent(key);
        return;
    }

    document.getElementById("emsta-package-page-contents-" + key).setAttribute("loaded", "true");

    if (statistics.warnings) alert(statistics.warnings);

    // if haven't done so yet, build the tab holder for this package
    if (!document.getElementById("emsta-page-tabContainer-" + key)) {
        var tabContainer = document.getElementById("TabHeaders");
        var newBar = document.createElement("div");
        newBar.id = "emsta-page-tabContainer-" + key;
        newBar.className = "emsta-page-tabContainer";
        tabContainer.appendChild(newBar);
    }

    // Generate the Content Tabs
    for (var currentPage = 0; currentPage < statistics.results.displayPages.length; currentPage++) {
        var pageJSON = statistics.results.displayPages[currentPage];  // get the JSON object for each page
        var pageElement = emsta_MakeNewPage(currentPage, pageJSON, key);    // make a page and advance the id counter

        if (pageJSON.displayTables === null) continue;  // why would that happen? 
        for (var table = 0; table < pageJSON.displayTables.length; table++) {
            var tableJSON = pageJSON.displayTables[table];	// get the JSON object for each table
            emsta_AddNewTable(pageElement, tableJSON);	// make a new table in this page
        }
    }

    // make the export button for this package
    var StatisticsHeaderButtons = document.getElementById("StatisticsHeaderButtons");
    var exportButton = document.createElement("a");
    exportButton.className = "w3-button w3-button-32 w3-round-xlarge emsta-export";
    exportButton.id = "emsta-export-" + key;
    exportButton.style.zIndex = 10;
    exportButton.style.display = "inline-block";
    exportButton.style.backgroundColor = "transparent";
    exportButton.innerHTML = "<img src=\"save.png\"/>";
    exportButton.href = responderKey + "_SaveAsExcel?packageKey=" + key;
    StatisticsHeaderButtons.insertBefore(exportButton, StatisticsHeaderButtons.childNodes[0]);

    ShowStatsContent(key);
}

function emsta_TabButton_Click(tabId) {
    var i, emstaTabs, emstaTabButtons;
    var emstaTabs = document.getElementsByClassName("emsta-page-body");
    for (i = 0; i < emstaTabs.length; i++) {
        emstaTabs[i].style.display = "none";
    }
    var emstaTabButtons = document.getElementsByClassName("emsta-page-tab");
    for (i = 0; i < emstaTabButtons.length; i++) {
        emstaTabButtons[i].className = emstaTabButtons[i].className.replace(" w3-grey w3-hover-grey", "");
    }
    var emstaSelectedTabButtons = document.getElementsByClassName("emsta-page-tab-" + tabId);
    for (i = 0; i < emstaSelectedTabButtons.length; i++) {
        emstaSelectedTabButtons[i].className += " w3-grey w3-hover-grey";
    }
    var emstaTabBodies = document.getElementsByClassName(CONTAINER_BODY_ID_PREFIX + tabId)
    for (i = 0; i < emstaTabBodies.length; i++) {
        emstaTabBodies[i].style.display = "block";
    }
    currentTab = tabId;
}

// This function creates the Statistics Header
function emsta_MakeStatisticsHeader(info, key) {
    var emstaTitles = document.getElementsByClassName("emsta-statistics-title");
    for (i = 0; i < emstaTitles.length; i++) {
        emstaTitles[i].style.display = "none";
    }
    var statisticsHeader = document.getElementById("StatisticsHeader");
    var statisticsHeaderButtons = document.getElementById("StatisticsHeaderButtons");
    if (firstLoad && info.description && info.description.trim() != "") {
        var helpButton = document.createElement("button");
        helpButton.className = "w3-button w3-button-32 w3-round-xlarge";
        helpButton.style.zIndex = 10;
        helpButton.style.display = "inline-block";
        helpButton.style.backgroundColor = "transparent";
        helpButton.innerHTML = "<img src=\"help.png\"/>";
        helpButton.onclick = function () { embe_MessageBox(info.description); };
        statisticsHeaderButtons.appendChild(helpButton);
    }
    // if showing multiple packages, also add "save all"
    if (firstLoad) {
        var emstaPackageButtons = document.getElementsByClassName("emsta-package-button");
        if (emstaPackageButtons && emstaPackageButtons.length > 0) {
            var exportAllButton = document.createElement("a");
            exportAllButton.className = "w3-button w3-button-32 w3-round-xlarge";
            exportAllButton.style.zIndex = 10;
            exportAllButton.style.display = "inline-block";
            exportAllButton.style.backgroundColor = "transparent";
            exportAllButton.innerHTML = "<img src=\"save_all.png\"/>";
            exportAllButton.href = responderKey + "_SaveAsExcel";
            statisticsHeaderButtons.insertBefore(exportAllButton, statisticsHeaderButtons.childNodes[0]);
        }
    }

    var title = document.createElement("h3");
    title.className = "w3-display-container emsta-statistics-title";
    title.id = "emsta-statistics-title-" + key;
    title.innerHTML = emsta_SafeText(info.title);
    statisticsHeader.appendChild(title);

    emstaSubTitles = document.getElementsByClassName("emsta-statistics-subtitle");
    for (i = 0; i < emstaSubTitles.length; i++) {
        emstaSubTitles[i].style.display = "none";
    }
    var subTitle = document.createElement("h5");
    subTitle.innerHTML = emsta_SafeText(info.subtitle);
    subTitle.className = "emsta-statistics-subtitle";
    subTitle.id = "emsta-statistics-subtitle-" + key;
    statisticsHeader.appendChild(subTitle);

    if (firstLoad) {
        var loaderPane = document.getElementById("LoaderPane");
        loaderPane.style.display = "none";
        var mainContainer = document.getElementById("MainContainer");
        mainContainer.style.display = "block";
    }

    firstLoad = false;
}

// This function creates a new Tab (Header and Body container) and returns the tab body element
function emsta_MakeNewPage(tabId, pageJSON, key) {
    // Add Tab Header
    var tabContainer = document.getElementById("emsta-page-tabContainer-" + key);
    var newPageTab = document.createElement("button");
    //newPageTab.id = CONTAINER_HEAD_ID_PREFIX + tabId;
    newPageTab.onclick = function () { emsta_TabButton_Click(tabId); };
    newPageTab.innerHTML = emsta_SafeText(pageJSON.button);
    newPageTab.className = "w3-bar-item w3-button emsta-page-tab emsta-page-tab-" + tabId;
    tabContainer.appendChild(newPageTab);

    var emstaContents = document.getElementsByClassName("emsta-package-page-contents");
    for (i = 0; i < emstaContents.length; i++)
        emstaContents[i].style.display = "none";

    // Add Tab Body
    var content = document.getElementById("emsta-package-page-contents-" + key);

    var newPageBody = document.createElement("div");
    newPageBody.className = "emsta-page-body w3-auto " + CONTAINER_BODY_ID_PREFIX + tabId;
    newPageBody.style.display = "none";
    content.appendChild(newPageBody);

    var newPageHead = document.createElement("div");
    newPageHead.className = "emsta-page-head " + CONTAINER_BODY_ID_PREFIX + tabId;
    newPageHead.style.position = "relative";
    newPageBody.appendChild(newPageHead);

    // Add page help, title & subtitle
    if (pageJSON.description && pageJSON.description.trim() != "") {
        var helpButton = document.createElement("button");
        helpButton.className = "w3-display-right w3-button w3-button-20 w3-round-xlarge";
        helpButton.innerHTML = "<img src=\"help.png\"/>";
        helpButton.style.zIndex = 10;
        helpButton.style.display = "inline-block";
        helpButton.style.backgroundColor = "transparent";
        helpButton.onclick = function () { embe_MessageBox(pageJSON.description); };
        newPageHead.appendChild(helpButton);
    }
    var title = document.createElement("h3");
    title.className = "w3-display-container";
    title.innerHTML = emsta_SafeText(pageJSON.title);
    newPageHead.appendChild(title);
    var newSubTitle = document.createElement("h5");
    newSubTitle.innerHTML = emsta_SafeText(pageJSON.subtitle);
    newPageHead.appendChild(newSubTitle);

    return newPageBody;
}

// This function adds a new Table in a tab body
function emsta_AddNewTable(page, tableJSON) {
    var newTableHead = document.createElement("div");
    newTableHead.className = "emsta-table-heading";
    newTableHead.style.position = "relative";
    page.appendChild(newTableHead);

    // Add table title, subtitle & help
    if (tableJSON.description && tableJSON.description.trim() != "") {
        var helpButton = document.createElement("button");
        helpButton.className = "w3-display-right w3-button w3-button-20 w3-round-xlarge";
        helpButton.innerHTML = "<img src=\"help.png\"/>";
        helpButton.style.zIndex = 10;
        helpButton.style.display = "inline-block";
        helpButton.style.backgroundColor = "transparent";
        helpButton.onclick = function () { embe_MessageBox(tableJSON.description); };
        newTableHead.appendChild(helpButton);
    }
    var title = document.createElement("h4");
    title.innerHTML = emsta_SafeText(tableJSON.title);
    title.className = "w3-display-container";
    newTableHead.appendChild(title);
    var newSubTitle = document.createElement("h6");
    newSubTitle.innerHTML = emsta_SafeText(tableJSON.subtitle);
    newTableHead.appendChild(newSubTitle);

    // Add table (if required)
    if (tableJSON.graph === null || tableJSON.graph === undefined || tableJSON.graph.showTable === undefined || tableJSON.graph.showTable === true) {
        var tableHolder = document.createElement("div");
        tableHolder.className = "container";
        var table = document.createElement("table");
        table.className = "w3-table w3-bordered w3-margin-bottom";
        table.style.display = "block";
        var headerRow = document.createElement("tr");
        emsta_AddSeparators(headerRow, { hasSeparatorBefore: true, hasSeparatorAfter: true }, { hasSeparatorBefore: true, hasSeparatorAfter: true });
        table.appendChild(headerRow);
        // first cell in headers is empty
        var topLeftCell = document.createElement("th");
        emsta_AddSeparators(topLeftCell, { hasSeparatorAfter: true }, null);
        headerRow.appendChild(topLeftCell);

        for (var h = 0; h < tableJSON.columns.length; h++) {
            var header = document.createElement("th");
            header.style.textAlign = "center";
            header.innerHTML = emsta_SafeText(tableJSON.columns[h].title);
            emsta_AddTooltip(header, tableJSON.columns[h].tooltip);
            emsta_AddSeparators(header, tableJSON.columns[h], null);
            emsta_SetColours(row, tableJSON.columns[h], null, null);
            headerRow.appendChild(header);
        }
        for (var r = 0; r < tableJSON.rows.length; r++) {
            var row = document.createElement("tr");
            emsta_AddSeparators(row, { hasSeparatorBefore: true, hasSeparatorAfter: true }, tableJSON.rows[r]);
            emsta_SetColours(row, null, tableJSON.rows[r], null);
            if (r == tableJSON.rows.length - 1) emsta_AddSeparators(row, null, { hasSeparatorAfter: true });
            var rowheader = document.createElement("td");
            rowheader.style.whiteSpace = "nowrap";
            rowheader.style.textAlign = "center";
            var rowStrong = tableJSON.rows[r].strong;
            rowheader.innerHTML = emsta_SafeText(rowStrong ? emsta_MakeStrong(tableJSON.rows[r].title) : tableJSON.rows[r].title);
            emsta_AddTooltip(rowheader, tableJSON.rows[r].tooltip)
            emsta_AddSeparators(rowheader, { hasSeparatorAfter: true }, tableJSON.rows[r]);
            row.appendChild(rowheader);

            for (var c = 0; c < tableJSON.columns.length; c++) {
                var cell = document.createElement("td");
                cell.style.whiteSpace = "nowrap";
                cell.style.textAlign = "center";
                cell.innerHTML = (rowStrong ? emsta_MakeStrong(tableJSON.cells[r][c].displayValue) : tableJSON.cells[r][c].displayValue);
                emsta_AddTooltip(cell, tableJSON.cells[r][c].tooltip)
                emsta_AddSeparators(cell, tableJSON.columns[c], tableJSON.rows[r]);
                emsta_SetColours(cell, tableJSON.columns[c], tableJSON.rows[r], tableJSON.cells[r][c]);
                row.appendChild(cell);
            }

            table.appendChild(row);
        }

        tableHolder.appendChild(table);
        page.appendChild(tableHolder);
    }

    // Add graph (if available)
    if (tableJSON.graph !== null && tableJSON.graph !== undefined && tableJSON.graph.allSeries !== null && tableJSON.graph.allSeries !== undefined) {
        var graphHolder = document.createElement("div");
        graphHolder.className = "container";
        var canvas = document.createElement("canvas");
        graphHolder.appendChild(canvas);
        page.appendChild(graphHolder);
        var chartData = emsta_GetChartData(tableJSON);
        var ctx = canvas.getContext('2d');
        var myChart = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: {
                scales: {
                    xAxes: [{
                        ticks: { beginAtZero: tableJSON.graph.axisX.startFromZero },
                        stacked: chartData.datasets.filter(function (d) { return d.stack; }).length > 0
                    }],
                    yAxes: [{
                        ticks: { beginAtZero: tableJSON.graph.axisY.startFromZero }
                    }]
                },
                legend: {
                    display: tableJSON.graph.legend.visible,
                    position: getLegendDocking(tableJSON.graph.legend)
                },
                onResize: handleChartResize
            }
        });
        myChart.graphSettings = tableJSON.graph;
        if (tableJSON.graph.title) {
            myChart.options.title = {
                text: tableJSON.graph.title,
                display: true,
                fontSize: 18,
                fontStyle: "normal"
            };
        }

        if (tableJSON.graph.axisX) {
            if (tableJSON.graph.axisX.label) {
                myChart.options.scales.xAxes[0].scaleLabel = {
                    display: true,
                    labelString: tableJSON.graph.axisX.label
                };
            }
            if (tableJSON.graph.axisX.interval > 0) {
                myChart.options.scales.xAxes[0].ticks.stepSize = tableJSON.graph.axisX.interval;
            }
            if (tableJSON.graph.axisX.valuesFrom) {
                myChart.options.scales.xAxes[0].type = 'linear';
                if (tableJSON.graph.seriesInRows) {

                } else {
                    var axisLabelPos = 0;
                    for (var i = 0; i < tableJSON.columns.length; i++)
                        if (tableJSON.columns[i].title == tableJSON.graph.axisY.valuesFrom)
                            axisLabelPos = i;
                    var max = tableJSON.cells[tableJSON.rows.length - 1][axisLabelPos].displayValue.replace(',', '');
                    myChart.options.scales.xAxes[0].ticks.max = (tableJSON.graph.axisX.interval > 0) ?
                        Math.ceil(max / tableJSON.graph.axisX.interval) * tableJSON.graph.axisX.interval
                        : max;
                }
            }
        }

        if (tableJSON.graph.axisY) {
            if (tableJSON.graph.axisY.label) {
                myChart.options.scales.yAxes[0].scaleLabel = {
                    display: true,
                    labelString: tableJSON.graph.axisY.label
                };
            }
            if (tableJSON.graph.axisY.interval > 0) {
                myChart.options.scales.yAxes[0].ticks.stepSize = tableJSON.graph.axisY.interval;
            }
            if (tableJSON.graph.axisY.valuesFrom) {
                myChart.options.scales.yAxes[0].type = 'linear';
            }
        }
    }
}

function emsta_AddTooltip(cell, tooltip) {
    if (tooltip) {
        cell.className = "tooltip-nw";
        cell.style.position = "relative";
        cell.setAttribute("data-tlite", emsta_SafeText(tooltip));
    }
}

function getLegendDocking(legend) {
    if (legend && legend.docking)
        if (legend.docking.toLowerCase() == "top" || legend.docking.toLowerCase() == "right" || legend.docking.toLowerCase() == "left" || legend.docking.toLowerCase() == "bottom") return legend.docking.toLowerCase();
    return "right";
}

function emsta_AddSeparators(cell, column, row) {
    if (column && column.hasSeparatorBefore) {
        cell.style.borderLeftWidth = "1px";
        cell.style.borderLeftStyle = "solid";
        cell.style.borderLeftColor = "black";
    }
    if (column && column.hasSeparatorAfter) {
        cell.style.borderRightWidth = "1px";
        cell.style.borderRightStyle = "solid";
        cell.style.borderRightColor = "black";
    }
    if (row && row.hasSeparatorBefore) {
        cell.style.borderTopWidth = "1px";
        cell.style.borderTopStyle = "solid";
        cell.style.borderTopColor = "black";
    }
    if (row && row.hasSeparatorAfter) {
        cell.style.borderBottomWidth = "1px";
        cell.style.borderBottomStyle = "solid";
        cell.style.borderBottomColor = "black";
    }
}

function emsta_SetColours(cell, column, row, cells) {
    if (cells && cells.foregroundColour) cell.style.color = cells.foregroundColour;
    else if (row && row.foregroundColour) cell.style.color = row.foregroundColour;
    else if (column && column.foregroundColour) cell.style.color = column.foregroundColour;

    if (cells && cells.backgroundColour) cell.style.backgroundColor = cells.backgroundColour;
    else if (row && row.backgroundColour) cell.style.backgroundColor = row.backgroundColour;
    else if (column && column.backgroundColour) cell.style.backgroundColor = column.backgroundColour;
}

function emsta_GetChartData(tableJSON) {
    var chartData = {};
    if (tableJSON.graph.allSeries == 0) return chartData;
    chartData.type = "bar";
    chartData.labels = [];
    if (tableJSON.graph.seriesInRows) {
        if (!tableJSON.graph.axisX.valuesFrom)
            for (var i = 0; i < tableJSON.columns.length; i++)
                chartData.labels[i] = translateNewLines(emsta_SafeText(tableJSON.columns[i].title));
    }
    else {
        if (!tableJSON.graph.axisX.valuesFrom)
            for (var i = 0; i < tableJSON.rows.length; i++)
                chartData.labels[i] = translateNewLines(emsta_SafeText(tableJSON.rows[i].title));
    }
    allDdatasets = [];
    for (var i = 0; i < tableJSON.graph.allSeries.length; i++)
        if (tableJSON.graph.allSeries[i].visible)
            allDdatasets[allDdatasets.length] = emsta_GetDatasetObject(tableJSON, tableJSON.graph.allSeries[i], i);
    chartData.datasets = allDdatasets.sort(function (a, b) { return (a.stack == null && b.stack != null) ? -1 : (b.stack == null && a.stack != null) ? 1 : 0 });
    return chartData;
}

function translateNewLines(x) {
    return x.split(/<br ?[/]?>/);
}

function emsta_GetChartType(type) {
    switch (type) {
        case "StackedColumn": return "bar";
        case "Point": return "line";
    }
    return "bar";
}

function emsta_GetDatasetObject(tableJSON, serie, sno) {
    var datasetObject = {};
    datasetObject.label = serie.name;
    if (serie.colour !== undefined) {
        datasetObject.borderColor = serie.colour;
        datasetObject.backgroundColor = serie.colour;
        datasetObject.hoverBackgroundColor = serie.colour;
    }
    switch (serie.type) {
        case "StackedColumn":
            datasetObject.type = "bar";
            datasetObject.stack = "myStack";
            break;
        case "ColumnClustered":
            datasetObject.type = "bar";
            break;
        case "Point":
            datasetObject.type = "line";
            datasetObject.showLine = false;
            datasetObject.fill = false;
            datasetObject.pointStyle = emsta_GetPointStyle(serie.markerStyle.toLowerCase());
            if (serie.size !== undefined) datasetObject.pointRadius = serie.size / 2;
            if (serie.colour !== undefined) {
                datasetObject.pointBorderColor = serie.colour;
                datasetObject.pointBackgroundColor = serie.colour;
            }
            break;
        case "Line":
            datasetObject.type = "line";
            datasetObject.showLine = true;
            datasetObject.fill = false;
            datasetObject.pointStyle = emsta_GetPointStyle(serie.markerStyle.toLowerCase());
            if (serie.size !== undefined) datasetObject.pointRadius = serie.size / 2;
            if (serie.colour !== undefined) {
                datasetObject.pointBorderColor = serie.colour;
                datasetObject.pointBackgroundColor = serie.colour;
            }
            break;
    }
    datasetObject.data = [];
    var axisLabelPos = 0;
    if (tableJSON.graph.seriesInRows) {
        if (tableJSON.graph.axisX.valuesFrom)
            for (var i = 0; i < tableJSON.rows.length; i++)
                if (tableJSON.rows[i].title == tableJSON.graph.axisX.valuesFrom)
                    axisLabelPos = i;
        for (var i = 0; i < tableJSON.rows.length; i++)
            if (tableJSON.rows[i].title == serie.name)
                for (var j = 0; j < tableJSON.columns.length; j++)
                    if (tableJSON.graph.axisX.valuesFrom)
                        datasetObject.data[j] = { y: tableJSON.cells[i][j].value, x: tableJSON.cells[axisLabelPos][j].value };
                    else
                        datasetObject.data[j] = tableJSON.cells[i][j].value;
    }
    else {
        if (tableJSON.graph.axisY.valuesFrom)
            for (var i = 0; i < tableJSON.columns.length; i++)
                if (tableJSON.columns[i].title == tableJSON.graph.axisY.valuesFrom)
                    axisLabelPos = i;
        for (var i = 0; i < tableJSON.columns.length; i++)
            if (tableJSON.columns[i].title == serie.name)
                for (var j = 0; j < tableJSON.rows.length; j++)
                    if (tableJSON.graph.axisX.valuesFrom)
                        datasetObject.data[j] = { y: tableJSON.cells[j][i].value, x: tableJSON.cells[j][axisLabelPos].value };
                    else
                        datasetObject.data[j] = tableJSON.cells[j][i].value;
    }
    return datasetObject;
}

function emsta_GetPointStyle(style) {
    var a = "rect";
    switch (style) {
        case "circle":
        case "cross":
        case "triangle":
            a = style;
            break;
        case "diamond": a = "rectRounded"; break;
        case "square": a = "rect"; break;
        case "star4":
        case "star5":
        case "star6":
        case "star10": a = "star";
    }
    return a;
}

function emsta_MakeStrong(title) {
    return "<strong>" + title + "</strong>";
}

function emsta_SafeText(text) {
    return text.replace("\n", "<br>").replace("&&", "&amp;");
}

/*
var callBackSave = '';
function emsta_SaveStatistics_Click(_callBackSave) {
    callBackSave = _callBackSave;
    embe_StringInputBox_Open('Name to be used for reloading Statistics', emsta_SaveStatistics);
}

function emsta_SaveStatistics(dialogResult, storeName) {
    if (dialogResult == false) return;
    alert("do sth with '" + callBackSave + "'");
}
*/

function resizeStatisticsWindow() {
    var allHeaders = document.getElementById("AllHeaders");
    var packageButtons = document.getElementById("PackageButtons");
    var allContent = document.getElementById("TabContent");
    allContent.style.top = allHeaders.offsetHeight + "px";
    allContent.style.bottom = packageButtons.offsetHeight + "px";
}

function handleChartResize(chart, size) {
    // resize, move or hide the graph legend so that it does not push the main graph out of existence! 
    chart.options.legend.display = (size.width > 500 && chart.graphSettings.legend.visible);
    if (chart.options.legend.display) {
        if (size.width > 1000) chart.options.legend.labels.fontSize = 12;
        else if (size.width < 1000 && size.width > 900) chart.options.legend.labels.fontSize = 11;
        else if (size.width < 900 && size.width > 800) chart.options.legend.labels.fontSize = 10;
        else if (size.width < 800 && size.width > 750) chart.options.legend.labels.fontSize = 9;
        else if (size.width < 750) chart.options.legend.labels.fontSize = 8;
        chart.options.legend.position = (size.width < 700 && getLegendDocking(chart.graphSettings.legend) != "top") ? "bottom" : getLegendDocking(chart.graphSettings.legend);
    }
    chart.update();
}