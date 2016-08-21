/*
 * Author: Abdullah A Almsaeed
 * Date: 4 Jan 2014
 * Description:
 *      This is a demo file used only for the main dashboard (index.html)
 **/

$(function () {

    "use strict";

    //Make the dashboard widgets sortable Using jquery UI
    $(".connectedSortable").sortable({
        placeholder: "sort-highlight",
        connectWith: ".connectedSortable",
        handle: ".box-header, .nav-tabs",
        forcePlaceholderSize: true,
        zIndex: 999999
    });
    
    $(".connectedSortable .box-header, .connectedSortable .nav-tabs-custom").css("cursor", "move");

    //jQuery UI sortable for the todo list
    $(".todo-list").sortable({
        placeholder: "sort-highlight",
        handle: ".handle",
        forcePlaceholderSize: true,
        zIndex: 999999
    });

    //bootstrap WYSIHTML5 - text editor
    $(".textarea").wysihtml5();

    $('.daterange').daterangepicker({
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        },
        startDate: moment().subtract(29, 'days'),
        endDate: moment()
    }, function (start, end) {
        window.alert("You chose: " + start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
    });

    /* jQueryKnob */
    $(".knob").knob();

    //jvectormap data
    var visitorsData = {
        "US": 398, //USA
        "SA": 400, //Saudi Arabia
        "CA": 1000, //Canada
        "DE": 500, //Germany
        "FR": 760, //France
        "CN": 300, //China
        "AU": 700, //Australia
        "BR": 600, //Brazil
        "IN": 800, //India
        "GB": 320, //Great Britain
        "RU": 3000 //Russia
    };
    
    //World map by jvectormap
    $('#world-map').vectorMap({
        map: 'world_mill_en',
        backgroundColor: "transparent",
        regionStyle: {
            initial: {
                fill: '#e4e4e4',
                "fill-opacity": 1,
                stroke: 'none',
                "stroke-width": 0,
                "stroke-opacity": 1}},
        series: {
            regions: [{
                values: visitorsData,
                scale: ["#92c1dc", "#ebf4f9"],
                normalizeFunction: 'polynomial'
            }]
        },
        onRegionLabelShow: function (e, el, code) {
            if (typeof visitorsData[code] != "undefined")
                el.html(el.html() + ': ' + visitorsData[code] + ' new visitors');
        }
    });

    //Sparkline charts
    var myvalues = [1000, 1200, 920, 927, 931, 1027, 819, 930, 1021];
    $('#sparkline-1').sparkline(myvalues, {
        type: 'line',
        lineColor: '#92c1dc',
        fillColor: "#ebf4f9",
        height: '50',
        width: '80'
    });
    myvalues = [515, 519, 520, 522, 652, 810, 370, 627, 319, 630, 921];
    $('#sparkline-2').sparkline(myvalues, {
        type: 'line',
        lineColor: '#92c1dc',
        fillColor: "#ebf4f9",
        height: '50',
        width: '80'
    });
    myvalues = [15, 19, 20, 22, 33, 27, 31, 27, 19, 30, 21];
    $('#sparkline-3').sparkline(myvalues, {
        type: 'line',
        lineColor: '#92c1dc',
        fillColor: "#ebf4f9",
        height: '50',
        width: '80'
    });

    <!--------------------------------------General Method------------------------------------>
    var HC = "HC";
    var TC = "TC";
    var BS = "BS";
    var DT = "DT";
    var JK = "JK";
    
    function getCandidatesData(startDate, endDate, func){
        $.ajax({
            url: "http://localhost:63486/api/Tweet?startDate="+ startDate + "&endDate=" + endDate,
            type: "GET",
            crossDomain: true,
            success: function (response) {
                func(response);
            },
        });
    };
    
    function getTodayDate(){
        var today = new Date();
        var dd = today.getDate()-1;
        var mm = today.getMonth()+1; //January is 0!
        var yyyy = today.getFullYear();

        if(dd<10) {
            dd='0'+dd
        } 

        if(mm<10) {
            mm='0'+mm
        } 

        today = yyyy+'-'+mm+'-'+dd;
        return today ;
    };

    function getLastWeeksDate(){
        var today = new Date();
        var lastWeek = new Date(today.getFullYear(), today.getMonth(), today.getDate()-14);
  
        var dd = lastWeek.getDate();
        var mm = lastWeek.getMonth()+1; //January is 0!
        var yyyy = lastWeek.getFullYear();

        if(dd<10) {
            dd='0'+dd
        } 

        if(mm<10) {
            mm='0'+mm
        } 

        lastWeek = yyyy+'-'+mm+'-'+dd;
        return lastWeek ;
    };
    
    <!--------------------------------------Candidates Bar Data ------------------------------->
        function initBarData(){
            getCandidatesData(getTodayDate(), getTodayDate(), displayBarData);
        };
        
    var displayBarData = function(data){
        $("#HC").text(data[0].HC);             
        $("#DT").text(data[0].DT);
        $("#BS").text(data[0].BS);
        $("#JK").text(data[0].JK);
        $("#TC").text(data[0].TC);
    };
    
    initBarData();
    <!--------------------------------------Daily Report--------------------------------------->

        
    
//color:"#3c8dbc"
    var barChartOptions = {
        grid: {
            borderWidth: 1,
            borderColor: "#f3f3f3",
            tickColor: "#f3f3f3"
        },
        series: {
            bars: {
                show: true,
                barWidth: 0.5,
                align: "center"
            }
        },
        xaxis: {
            mode: "categories",
            tickLength: 0
        }
    };
    
    function firstInitDailyReport(){
        getCandidatesData(getTodayDate(), getTodayDate(), initDailtyReportFunc);
    };

    var initDailtyReportFunc = function(newData){
        var tmpData = [["HC",newData[0].HC],["BS",newData[0].BS],["TC",newData[0].TC],["DT",newData[0].DT],["JK",newData[0].JK]];
        $.plot("#daily-chart", [{
            data: tmpData,
            color:"#3c8dbc"
        }], barChartOptions);    
    }
    
    firstInitDailyReport();
      
    $("#calendar").datepicker({format: "yyyy-mm-dd"});
    
    $('#calendar').on("changeDate", function() {
        var selectedDate = $('#calendar').datepicker('getFormattedDate');
        getCandidatesData(selectedDate, selectedDate, initDailtyReportFunc);
        /*var tmpBarChart = barChart.getData();
        tmpBarChart[0].data = data;
        barChart.setData(tmpBarChart);
        barChart.draw();*/
    });
    
    <!-------------------------------------Trends Report--------------------------------------------------->
    
        var morrisCandidatesData;
    var morriGraph;

    var morrisData = {
        element: 'revenue-chart',
        resize: true,
        xkey: 'PredictionDate',
        ykeys: ['HC', 'BS', 'TC', 'DT', 'JK'],
        labels: ['Hillary Clinton', 'Bernie Sanders', 'Ted Cruz', 'Donald Trump', 'John Kasich'],
        lineColors: ['#00c0ef', '#dd4b39', '#605ca8', '#00a65a', '#f39c12'],
        hideHover: 'auto',
        xLabels: "day"
        //xLabelFormat:function(x){return x.toString();}
    };
    
    function drawTrendsReport(startDate, endDate, callback){        
        getCandidatesData(startDate, endDate, callback);
    };
    
    drawTrendsReport(getLastWeeksDate(), getTodayDate(), initMorrisFunc);

    var initMorrisFunc = function(data){
        morrisData.data = data;
        morrisGraph = Morris.Line(morrisData);    
    };
    
    var reDrawMorrisFunc = function(data){
        $("#revenue-chart").empty();
        morrisData.data = data;
        Morris.Line(morrisData);
        //morrisGraph.resize();
        //  (new Morris()).Line(morrisData);    
    };
    
    function checkDates()
    {
        var startDate = $("#startDate").val();
        var endDate = $("#endDate").val();
        
        if (startDate!=null || endDate != null){
            alert ("Date field is missing");
        }else{
            return;
        }
    };
    
    $("#startDate").change(function(){
        //checkDates();       
        drawTrendsReport($("#startDate").val(),$("#endDate").val(), reDrawMorrisFunc);
    });
    
    $("#endDate").change(function(){
        //checkDates();       
        drawTrendsReport($("#startDate").val(),$("#endDate").val(), reDrawMorrisFunc);
    });

});