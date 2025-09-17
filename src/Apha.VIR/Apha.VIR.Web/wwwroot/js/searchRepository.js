$(function () {
    // Run once when page loads
    $(document).ready(function () {
        $('.characteristic-block').each(function () {            
            var $block = $(this);
            var characteristics = $block.find('.characteristic-ddl').val();            
            var type = $block.find('.characteristic-ddl-type').val();    
            if (characteristics != null && characteristics != "") {
                var comparatorSelect = $block.find('.comparator-ddl');
                comparatorSelect.show();
                var comparator = comparatorSelect.val();
                showHideControlsOnPageLoad($block, type, comparator);
            }            
        });
    });

    $('#ddlVirusFamily').change(function () {
        var virusFamilyId = $(this).val();
        $.get('/SearchRepository/GetVirusTypesByVirusFamily', { virusFamilyId: virusFamilyId }, function (virusTypes) {
            var $virusTypeDdl = $('#ddlVirusType');
            $virusTypeDdl.empty();
            $virusTypeDdl.append($('<option>').text('').attr('value', ''));
            $.each(virusTypes, function (i, type) {
                $virusTypeDdl.append($('<option>').text(type.text).attr('value', type.value));
            });

            rebuildCharacteristicsDropdowns(null);         
        });
    });

    $('#ddlGroup').change(function () {
        var hostSpicyId = $(this).val();
        $.get('/SearchRepository/GetHostBreedsByGroup', { hostSpicyId: hostSpicyId }, function (hostBreeds) {
            var $hostBreedDdl = $('#ddlSpecies');
            $hostBreedDdl.empty();
            $hostBreedDdl.append($('<option>').text('').attr('value', ''));
            $.each(hostBreeds, function (i, breed) {
                $hostBreedDdl.append($('<option>').text(breed.text).attr('value', breed.value));
            });
        });
    });

    $('#ddlVirusType').change(function () {
        var virusTypeId = $(this).val();
        rebuildCharacteristicsDropdowns(virusTypeId);
    });

    $('.characteristic-ddl').change(function () {        
        var $charddlBlock = $(this).closest('.characteristic-block');
        var characteristicId = $(this).val();
        var type = $(this).find('option:selected').data('type');
        if (characteristicId) {
            $.get('/SearchRepository/GetComparatorsAndListValues', { virusCharacteristicId: characteristicId }, function (data) {
                var $comparatorDdl = $charddlBlock.find('.comparator-ddl');
                $comparatorDdl.empty();
                $.each(data.comparators, function (i, comp) {
                    $comparatorDdl.append($('<option>').text(comp.text).attr('value', comp.value));
                });

                $comparatorDdl.show();
                var $listValuesDdl = $charddlBlock.find('.char-list-values-ddl');
                $listValuesDdl.empty();
                $listValuesDdl.append($('<option>').text('').attr('value', ''));
                $.each(data.listValues, function (i, listVal) {
                    $listValuesDdl.append($('<option>').text(listVal.text).attr('value', listVal.value));
                });
                $listValuesDdl.show();

                updateControlsOnCharacteristicChange($charddlBlock, type);
            });
        }
        else {
            resetCharactristicBlock($charddlBlock);
        }
    });

    $('.comparator-ddl').change(function () {
        var $charddlBlock = $(this).closest('.characteristic-block');
        var type = $charddlBlock.find('.characteristic-ddl').find('option:selected').data('type');
        var comparator = $(this).val();
        updateControlsOnComparatorChange($charddlBlock, type, comparator);
    });

    $(document).on('click','.pagination a', function (e) {
        e.preventDefault();
        var pageNo = $(this).data('pageno');
        $.get('BindIsolateGridOnPaginationAndSort', { pageNo: pageNo }, function (htmlData) {
            $('#gridIsolateResult').html(htmlData);
        });
    });

    $(document).on('click', '#SearchRepositoryGrid th a', function (e) {
        e.preventDefault();        
        var sortcolumn = $(this).data('sortcolumn');
        var sortorder = Boolean(parseInt($(this).data('sortorder')));
        $.get('BindIsolateGridOnPaginationAndSort', { pageNo: 0, column: sortcolumn, sortOrder: sortorder }, function (htmlData) {
            $('#gridIsolateResult').html(htmlData);
        });
    });

    function rebuildCharacteristicsDropdowns(virusTypeId) {        
        $.get('/SearchRepository/GetVirusCharacteristicsByVirusType', { virusTypeId: virusTypeId }, function (virusCharacteristics) {
            $('.characteristic-block').each(function (index, block) {                
                var $charddlBlock = $(block);
                var $characteristicDdl = $charddlBlock.find('.characteristic-ddl');
                $characteristicDdl.empty();
                $characteristicDdl.append($('<option>').text('').attr('value', ''));
                $.each(virusCharacteristics, function (i, char) {
                    $characteristicDdl.append($('<option>').text(char.text).attr('value', char.value).attr('data-type', char.dataType));
                });

                resetCharactristicBlock($charddlBlock);
            });
        });
    }

    function resetCharactristicBlock($block) {
        $block.find('.comparator-ddl').empty().hide();
        $block.find('.char-list-values-ddl').empty().hide();
        $block.find('.char-value1').val('').hide();
        $block.find('.char-value2').val('').hide();
        $block.find('.char-and-label').hide();
    }

    function updateControlsOnCharacteristicChange($block, type) {
        var $listValues = $block.find('.char-list-values-ddl');
        var $value1 = $block.find('.char-value1');
        var $value2 = $block.find('.char-value2');
        var $andLabel = $block.find('.char-and-label');
        $block.find('.characteristic-ddl-type').val(type);

        if (type === "Numeric") {
            $listValues.empty().hide();
            $value1.val('').show();
            $value2.val('').hide();
            $andLabel.hide();
        } else if (type === "SingleList") {
            $listValues.show()
            $value1.val('').hide();
            $value2.val('').hide();
            $andLabel.hide();
        } else if (type === "Yes/No") {
            $listValues.show()
            $value1.val('').hide();
            $value2.val('').hide();
            $andLabel.hide();
        } else if (type === "Text") {
            $listValues.empty().hide()
            $value1.val('').show();
            $value2.val('').hide();
            $andLabel.hide();
        }
    }

    function updateControlsOnComparatorChange($block, type, comparator) {
        var $listValues = $block.find('.char-list-values-ddl');
        var $value1 = $block.find('.char-value1');
        var $value2 = $block.find('.char-value2');
        var $andLabel = $block.find('.char-and-label');

        if (comparator === "between") {
            $listValues.hide()
            $value1.val('').show();
            $value2.val('').show();
            $andLabel.show();
        } else if (comparator === "begins with") {
            $listValues.hide()
            $value1.val('').show();
            $value2.val('').hide();
            $andLabel.hide();
        } else if (comparator === "=" || comparator === "not equal to") {
            if (type === "Numeric" || type === "Text") {
                $listValues.hide()
                $value1.val('').show();
                $value2.val('').hide();
                $andLabel.hide();
            }
            else {
                $listValues.show()
                $value1.val('').hide();
                $value2.val('').hide();
                $andLabel.hide();
            }
        } else {
            $value1.show();
            $value2.val('').hide();
            $andLabel.hide();
        }       
    } 

    function showHideControlsOnPageLoad($block, type, comparator) {
        var $listValues = $block.find('.char-list-values-ddl');
        var $value1 = $block.find('.char-value1');
        var $value2 = $block.find('.char-value2');
        var $andLabel = $block.find('.char-and-label');

        if (comparator === "between") {
            $listValues.hide()
            $value1.show();
            $value2.show();
            $andLabel.show();
        } else if (comparator === "begins with") {
            $listValues.hide()
            $value1.show();
            $value2.hide();
            $andLabel.hide();
        } else if (comparator === "=" || comparator === "not equal to") {
            if (type === "Numeric" || type === "Text") {
                $listValues.hide()
                $value1.show();
                $value2.hide();
                $andLabel.hide();
            }
            else {
                $listValues.show()
                $value1.hide();
                $value2.hide();
                $andLabel.hide();
            }
        } else {
            $value1.show();
            $value2.hide();
            $andLabel.hide();
        }
    }
});

function ClearSearchFilter() {
    window.location.href = '/SearchRepository/Index';    
}