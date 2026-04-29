using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public enum CourseLevel
{
    [Display(Name = "Початковий")]
    Pochatkovyi,
    [Display(Name = "Середній")]
    Serednii,
    [Display(Name = "Просунутий")]
    Prosunutyi
}

