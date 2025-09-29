using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models.Lookup;

public class LookupItemModel
{
    public Guid Id { get; set; } = Guid.Empty;
    [Required(ErrorMessage = "Name must be enetered.")]
    public string Name { get; set; } = null!;
    public Guid? Parent { get; set; }
    public string? ParentName { get; set; }
    public string? AlternateName { get; set; }
    public bool Active { get; set; } = false;
    public bool Sms { get; set; } = false;
    public string? Smscode { get; set; }
    public byte[] LastModified { get; set; } = null!;

    public IEnumerable<ValidationResult> ValidateLookUpItemAdd(ValidationContext validationContext,
        IEnumerable<LookupItemModel> lookupItemList, bool parentExist = false)
    {
        var results = new List<ValidationResult>();
        bool isDuplicate;

        if (Name == string.Empty)
        {
            results.Add(new ValidationResult("Name not specified for this item."));
        }

        //Ensure that the parent is chosen (if required)
        if (parentExist && (Parent == Guid.Empty || Parent == null))
        {
            results.Add(new ValidationResult("Parent item not specified for this item"));
        }

        //Ensure the item does not already exist
        if (parentExist)
        {
            isDuplicate = lookupItemList.Any(listItem => listItem.Id == Id || listItem.Name == Name);
        }
        else
        {
            isDuplicate = lookupItemList.Any(listItem => listItem.Id == Id || (listItem.Name == Name && listItem.Parent == Parent));
        }

        if (isDuplicate)
        {
            results.Add(new ValidationResult("Item already exists."));
        }

        return results;
    }
    public IEnumerable<ValidationResult> ValidateLookUpItemUpdate(ValidationContext validationContext,
        IEnumerable<LookupItemModel> lookupItemList, bool parentExist = false, bool isIteminUse = false)
    {
        var results = new List<ValidationResult>();
        bool isDuplicate;

        if (Id == Guid.Empty)
        {
            results.Add(new ValidationResult("Id not specified for this item."));
        }
        if (Name == string.Empty)
        {
            results.Add(new ValidationResult("Name not specified for this item."));
        }

        //Ensure that the parent is chosen (if required)
        if (parentExist && (Parent == Guid.Empty || Parent == null))
        {
            results.Add(new ValidationResult("Parent item not specified for this item"));
        }

        //Ensure the item already exists
        bool found = lookupItemList.Any(listItem => listItem.Id == Id);

        if (!found)
        {
            results.Add(new ValidationResult("Item does not exist."));
        }

        //Ensure that the item is unique
        if (!parentExist)
        {
            isDuplicate = lookupItemList.Any(listItem => listItem.Id != Id && listItem.Name == Name);
        }
        else
        {
            isDuplicate = lookupItemList.Any(listItem => listItem.Id != Id && (listItem.Name == Name && listItem.Parent == Parent));
        }

        if (isDuplicate)
        {
            results.Add(new ValidationResult("Item is not unique."));
        }

        if (!Active && isIteminUse)
        {
            results.Add(new ValidationResult("Item cannot be set to inactive status as it is already in use."));
        }
        return results;
    }

    public IEnumerable<ValidationResult> ValidateLookUpItemDelete(ValidationContext validationContext,
        IEnumerable<LookupItemModel> lookupItemList, bool isIteminUse = false)
    {
        var results = new List<ValidationResult>();

        //Ensure the item already exists
        bool found = lookupItemList.Any(listItem => listItem.Id == Id);

        if (!found)
        {
            results.Add(new ValidationResult("Item does not exist."));
        }

        if (isIteminUse)
        {
            results.Add(new ValidationResult("Item cannot be deleted as it is already in use. Please ensure that all records dependent on this item have been removed eg. characteristics"));
        }
        return results;
    }
}
