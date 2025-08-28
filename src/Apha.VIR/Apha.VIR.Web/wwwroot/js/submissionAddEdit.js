
function openPopupSender() {

    $.get("/Submission/GetSenderDetails?countryId=" + $('#SubmittingCountry').val(), function (data) {
        $('#popupModalContent').html(data);
        $("#popupModal .modal-dialog").css("max-width", "626px");

        var popup = new bootstrap.Modal(document.getElementById('popupModal'));
        popup.show();

        $('#popupModal .modal-dialog').css('margin-top', '205px');

        $('#popupModal').on('shown.bs.modal', function () {
            $(this).find('select, input, button').first().trigger('focus');
        });
    });
}

function openPopupOrganisation() {

    $.get("/Submission/GetOrganisationDetails?countryId=" + $('#SubmittingCountry').val(), function (data) {
        $('#popupModalContent').html(data);
        $("#popupModal .modal-dialog").css("max-width", "626px");

        var popup = new bootstrap.Modal(document.getElementById('popupModal'));
        popup.show();

        $('#popupModal .modal-dialog').css('margin-top', '205px');

        $('#popupModal').on('shown.bs.modal', function () {
            $(this).find('select, input, button').first().trigger('focus');
        });
    });
}

function openPopupAddSender() {
    $.get("/Submission/GetAddSender", function (data) {
        $("#popupModalContent").html(data);

        $("#popupModal .modal-dialog").css("max-width", "466px");

        // Re-parse validation for dynamically added form
        $.validator.unobtrusive.parse("#AddSenderPopupForm");

        $("#popupModal").modal("show");
        $('#popupModal .modal-dialog').css('margin-top', '125px');
    });
}

function closePopup() {
    $("#popupModal").modal("hide");
}

function submitSender() {
    debugger;
    var form = $("#AddSenderPopupForm");
    if (!form.valid()) {
        return;
    }
    var formData = form.serialize();
    $.post("/Submission/AddSender", formData, function (response) {
        if (response.success) {
            var Sender = $('#SenderName').val();
            var Organisation = $('#txtSenderOrganisation').val();
            var Address = $('#txtSenderAddress').val();
            var Country = $('#Country').val();
            $('#Sender').val(Sender);
            $('#SenderOrganisation').val(Organisation);
            $('#SenderAddress').val(Address);
            $('#SubmittingCountry').val(Country);
            closePopup();
        } else {
            alert(response.message);
        }
    });
}

function changedSubmittingCountry(submittingCounrty) {
    if ($(submittingCounrty).val() != "" && $('#CountryOfOrigin').val() == "") {
        $('#CountryOfOrigin').val($(submittingCounrty).val());
    }
}

function SenderPopupOkClick() {
    var ddl = $('#popupDropdown');
    var selectedOption = ddl.find("option:selected");
    var Sender = selectedOption.data("sendername");
    var Organisation = selectedOption.data("senderorganisation");
    var Address = selectedOption.data("senderaddress");
    var Country = selectedOption.data("country");

    $('#Sender').val(Sender);
    $('#SenderOrganisation').val(Organisation);
    $('#SenderAddress').val(Address);
    $('#SubmittingCountry').val(Country);

    var popup = bootstrap.Modal.getInstance(document.getElementById('popupModal'));
    popup.hide();
}

function OrganisationPopupOkClick() {
    var ddl = $('#popupDropdown');
    var selectedOption = ddl.find("option:selected");
    var Organisation = selectedOption.data("senderorganisation");
    var Address = selectedOption.data("senderaddress");
    var Sender = selectedOption.data("sendername");
    var Country = selectedOption.data("country");

    $('#Sender').val(Sender);
    $('#SenderOrganisation').val(Organisation);
    $('#SenderAddress').val(Address);
    $('#SubmittingCountry').val(Country);

    var popup = bootstrap.Modal.getInstance(document.getElementById('popupModal'));
    popup.hide();
}