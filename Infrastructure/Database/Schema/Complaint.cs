﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class Complaint : Audit
    {
        public Complaint(string title, string description, bool? isSolved)
        {
            Title = title;
            Description = description;
            IsSolved = isSolved;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool? IsSolved { get; set; }

        public void FromDomain(Domain.Complaints.Complaint domaincomplaint)
        {
            Title = domaincomplaint.Title;
            Description = domaincomplaint.Description;
        }

        public static Expression<Func<Complaint, Domain.Complaints.Complaint>> ToDomain =>
            complaint => new Domain.Complaints.Complaint(
                complaint.Id,
                complaint.Title,
                complaint.Description,
                complaint.IsSolved,
                complaint.CreatedByUserId,
                complaint.UpdatedByUserId
            );

    }
}
