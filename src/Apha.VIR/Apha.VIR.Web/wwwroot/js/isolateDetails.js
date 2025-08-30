$(function () {
    $('#ddlVirusFamily').change(function () {
        var virusFamilyId = $(this).val();
        $.get('/Isolates/GetVirusTypesByVirusFamily', { virusFamilyId: virusFamilyId }, function (virusTypes) {
            var $virusTypeDdl = $('#ddlVirusType');
            $virusTypeDdl.empty();           
            $.each(virusTypes, function (i, type) {
                $virusTypeDdl.append($('<option>')
                    .text(type.text).attr('value', type.value)
                    .attr('data-altname', type.dataType));
            });
            $("#ddlVirusType").trigger("change");
        });
    });

    $('#ddlFreezer').change(function () {
        var freezer = $(this).val();
        $.get('/Isolates/GetTraysByFeezer', { freezer: freezer }, function (freezers) {
            var $trayDdl = $('#ddlTray');
            $trayDdl.empty();
            $trayDdl.append($('<option>').text('').attr('value', ''));
            $.each(freezers, function (i, type) {
                $trayDdl.append($('<option>').text(type.text).attr('value', type.value));
            });
        });
    });

    $('#ValidToIssue').change(function () {
        var validToIssue = $(this);
        if (validToIssue.is(':checked')) {
            $('#WhyNotValidToIssue').prop('disabled', true).val('');
        }
        else {
            $('#WhyNotValidToIssue').prop('disabled', false);
        }
    });

    $("#ddlVirusType, #YearOfIsolation").on("change", function () {        
        debugger;
        var virusType = $('#ddlVirusType').find('option:selected').data('altname');
        if (virusType == "") {
           virusType = $('#ddlVirusType').val();
        }
        var yearOfIsolation = $('#YearOfIsolation').val();
        $.get('/Isolates/GenerateNomenclature', { avNumber: $('#AVNumber').val(), sampleId: $('#IsolateSampleId').val(), virusType: virusType, yearOfIsolation: yearOfIsolation }, function (nomenclature) {
            $("#spnCurrentNomenclature").text(nomenclature); 
            $("#CurrentNomenclature").val(nomenclature); 
        });
    });
});