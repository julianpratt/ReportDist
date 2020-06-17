// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


let data = [];

function handleErrors(response) {
  if (!response.ok) {
      throw Error(response.statusText);
  }
  return response;
}

function getStandingData(display, filter) {
  getData('/StandingData', display, filter);
} 

function getOrgTree(display, filter) {
  getData('/OrgTree', display, filter);
} 

function getData(uri, display, filter) {
  fetch(uri)
    .then(handleErrors)
    .then(response => response.json())
    .then(data => display(data, filter))
    .catch(error => console.error('Unable to get items.', error));
}

function displayHomeIndex(data, filter) {
    var $dropdown1 = $("#Company");
    data.companies.forEach(item => $dropdown1.append($("<option />").val(item.code).text(item.code)));

    var $dropdown2 = $("#Year");
    data.shortYears.forEach(item => $dropdown2.append($("<option />").val(item).text(item)));

    var $dropdown3 = $("#ReportType");
    $dropdown3.append($("<option />").val("").text(""));
    data.reportTypes.forEach(item => $dropdown3.append($("<option />").val(item.code).text(item.name)));

    searchInit(filter);
}

function displayReportCreate(data, dummy) {

  var $dropdown1 = $("#Company");
  data.companies.forEach(item => $dropdown1.append($("<option />").val(item.code).text(item.code)));
 
  var $dropdown2 = $("#Year");
  data.shortYears.forEach(item => $dropdown2.append($("<option />").val(item).text(item)));

  var $dropdown3 = $("#ReportType");
  data.reportTypes.forEach(item => $dropdown3.append($("<option />").val(item.code).text(item.name)));

}

function displayReportEdit(data, dummy) {

  var $dropdown1 = $("#AccessCode");
  $dropdown1.append($("<option />").val("!").text("Select a Division"));
  data.accessCodes.forEach(item => $dropdown1.append($("<option />").val(item.code).text(item.title)));

}

function displayCirculationEdit() {

  var $dropdown1 = $("#drpAddElecType");
  $dropdown1.append($("<option />").val("NONE").text("None"));
  $dropdown1.append($("<option />").val("SUMMARY").text("Summary Only"));
  $dropdown1.append($("<option />").val("FULL").text("Full Report"));
  $dropdown1.append($("<option />").val("CD").text("Summary with CD to follow"));
  setDelivery("Elec", elecDelivery);

  var $dropdown2 = $("#drpAddPaperType");
  $dropdown2.append($("<option />").val("NONE").text("None"));
  $dropdown2.append($("<option />").val("SUMMARY").text("Summary Only"));
  $dropdown2.append($("<option />").val("FULL").text("Full Report"));
  setDelivery("Paper", paperDelivery);

}

function setDelivery(code) {
  //console.log("Set Delivery to " + code);
  document.getElementById("drpAddElecType").selectedIndex = 0;
  document.getElementById("drpAddPaperType").selectedIndex = 0;
  if      (code == "ES") { document.getElementById("drpAddElecType").selectedIndex = 1; } 
  else if (code == "EF") { document.getElementById("drpAddElecType").selectedIndex = 2; }
  else if (code == "PS") { document.getElementById("drpAddPaperType").selectedIndex = 1; } 
  else if (code == "PF") { document.getElementById("drpAddPaperType").selectedIndex = 2; }
  /*  
    //$("#drpAddElecType").val(code).prop('selected', true);
  }
  else {
    document.getElementById("drpAddPaperType").selectedIndex = code; 
    //$("#drpAddPaperType").val(code).prop('selected', true);
  }
  */

}

function isValidCirculation() {
  
  if (document.getElementById("drpAddElecType").selectedIndex == 0 && document.getElementById("drpAddPaperType").selectedIndex == 0)
  {
    $("#ValidationMessage").text("Please select a method of report distribution for this recipient. Paper or Electronic, Summary or Full?");
    return false;
  }
  else if (document.getElementById("drpAddElecType").selectedIndex > 0 && $("#Email").val().trim().length == 0)
  {
    $("#ValidationMessage").text("A valid email address is required for this recipient, in order to distribute the report electronically.");
    return false;
  }
  else if (document.getElementById("drpAddPaperType").selectedIndex > 0 && $("#Address").val().trim().length == 0)
  {
    $("#ValidationMessage").text("A valid postal address is required for this recipient, in order to distribute the report.");
    return false;
  }
  
  $("#ValidationMessage").text("");
  return true;

}


function setAxess() {
  var vs = $("#AccessCode").val().trim();
  var v  = $("#Axess").val();

  if (vs == "!")
  {}
  else if (vs == "ALL" || vs == "NONE")
  {
    $("#Axess").val(vs);
  }
  else if (v == "ALL" || v == "NONE")
  {
    $("#Axess").val(vs);
  }
  else 
  {
    var codes = v.trim().split("/");
    if (!codes.includes(vs)) codes.push(vs);
    $("#Axess").val(codes.sort().join("/"));
  }
  $("#AccessCode").val("!");
  $("#btnAxessNONE").prop('disabled', false);
  $("#btnAxessALL").prop('disabled', false)
}

function setAxessNone() {
  $("#Axess").val("NONE");
  $("#btnAxessNONE").prop('disabled', true);
  $("#btnAxessALL").prop('disabled', false);
}

function setAxessAll() {
  $("#Axess").val("ALL");
  $("#btnAxessNONE").prop('disabled', false);
  $("#btnAxessALL").prop('disabled', true);
}

function searchInit(filter) {
  var parts = filter.split("-");
  if (parts[0] == "search")
  {
    var start = parts[1].split("/");
    $("#Company").val(start[0]).prop('selected', true);
    $("#Year").val(start[1]).prop('selected', true);
    var code = start[2];
    var i = 3;
    while (i < start.length) { code = code + "/" + start[i]; ++i; }
    $("#Code").val(code);
    $("#Number").val(parts[2]);
    $("#ReportType").val(parts[3]).prop('selected', true);
  }
}

function validateSearch() {
  var num = $("#Number").val();
  if (isNaN(num)) { 
    alert("Report number must be an integer");
    return false;
  }
}

function validateNDTNo() {
  if ($("#CheckNDT").prop("checked")) {
    var num = $("#NDTNo").val();
    if (num == null || num.length == 0 || isNaN(num)) { 
      alert("NDT No must be an integer");
      return false;
    }
  }
}

function reportDelete(id) {
  if (confirm('Are you sure you want to delete this report?')) {
    window.location.href = '/Report/Delete/' + id;
  }
}

function onCheck() {
  if (canCommit()) {
    $("#CannotCommit").hide();
    if ($("#chkQAConfirm").is(":checked")) {
      $("#btnCommit").show();
    }
    else if ($("#chkQAConfirm2").is(":checked")) {
      $("#btnCommit").show();
    }
    else {
      $("#btnCommit").hide();
    }
  }
  else {
    $("#CannotCommit").show();
    $("#btnCommit").hide();
  }
}

function canCommit() {
  title    = $("#Title").val().trim();
  abstract = $("#Abstract").val().trim();
  author   = $("#Author").val().trim();
  axess    = $("#Axess").val().trim();
  if (title.length > 0 && abstract.length > 0 && author.length > 0 && axess.length > 0 ) return true;
  else                                                                                   return false;
} 

function saveOrgTree(data, dummy) {
  orgTree = data;
  fillDivision();
}

function fillDivision() {

  var $dropdown = $("#Division");
  orgTree.divisions.forEach(item => $dropdown.append($("<option />").val(item.code).text(item.name)));

  $('#Division').attr('size', $('#Division option').length);
  $("#Department").hide();
  $("#Team").hide();
  $("#Company").hide();
  $("#NDTNo").hide();

}

function onclickDivision() {
  divcode = $("#Division").val();
  orgTree.divisions.forEach(item => fillDepartment(item, divcode));
  setPreview(divcode);
}

function fillDepartment(div, divcode) {

  if (div.code != divcode) return;

  division = div;

  var $dropdown = $("#Department");
  $dropdown.empty();
  div.departments.forEach(item => $dropdown.append($("<option />").val(item.code).text(item.name)));

  var l = $('#Department option').length;
  if (l > 0) {
    if (l == 1) l = 2; // Fix HTML display 'bug'
    $dropdown.attr('size', l);
    $dropdown.show();
  }
  else       $dropdown.hide();
  $("#Team").hide();
  $("#Team").empty();

}

function onclickDepartment() {
  depcode = $("#Department").val();
  division.departments.forEach(item => fillTeam(item, depcode));
  setPreview(depcode);
}

function fillTeam(dep, depcode) {

  if (dep.code != depcode) return;

  department = dep;

  var $dropdown = $("#Team");
  $dropdown.empty();
  dep.teams.forEach(item => $dropdown.append($("<option />").val(item.code).text(item.name)));

  var l = $('#Team option').length;
  if (l > 0) {
    if (l == 1) l = 2; // Fix HTML display 'bug'
    $dropdown.attr('size', l);
    $dropdown.show();
  }
  else $dropdown.hide();

}

function onclickTeam() {
  setPreview($("#Team").val());
}

function setPreview(code) {
  var company = document.getElementById('Company').value;
  var year = $("#Year").val();
  var reportno = company + "/" + year + "/" + code + "/1234/";
  $("#Preview").text(reportno);
}

function onclickCheckNDT() {
  if ($("#CheckNDT").prop("checked")) {
    $("#Company").show();
    $("#NDTNo").show();
    $("#CheckNDT").val(true);
  }
  else {
    $("#Company").hide();
    $("#NDTNo").hide();
    document.getElementById('Company').selectedIndex = 0;
    document.getElementById('NDTNo').value = null;
    $("#CheckNDT").val(false);
  }

}

function detectExploder() {
  var ua = window.navigator.userAgent;

  var msie = ua.indexOf('MSIE ');
  if (msie > 0) {
      // IE 10 or older 
      alert("Warning: You are using Internet Explorer 10 (or older). Please use a more recent browser for running this application.");
  }

  var trident = ua.indexOf('Trident/');
  if (trident > 0) {
      // IE 11 => return version number
      alert("Warning: You are using Internet Explorer 11. Please use a more recent browser for running this application.");
  }
} 

function readOnlyEditReport() {
  state = $("#State").val();
  if (state > 0) {
    $('.Field').attr('disabled', true);
  }
}
