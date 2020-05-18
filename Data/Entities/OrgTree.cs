using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    public class OrgTree
    {
        public OrgTree()
        {
            Divisions = new List<Division>();
        }
        public List<Division>   Divisions { get; set; }
    }

    public class Division
    {
        public Division(int id, string code, string name)
        {
            this.Id     = id;
            this.Code   = code;
            this.Name   = name;
            Departments = new List<Department>();
        }
        public int              Id          { get; private set; }
        public string           Code        { get; private set; }
        public string           Name        { get; private set; }
        public List<Department> Departments { get; set; }
    }

    public class Department
    {
        public Department(int id, string code, string name)
        {
            this.Id     = id;
            this.Code   = code;
            this.Name   = name;
            Teams = new List<Team>();
        }
        public int              Id          { get; private set; }
        public string           Code        { get; private set; }
        public string           Name        { get; private set; }
        public List<Team>       Teams       { get; private set; }
    }
    public class Team
    {
        public Team(int id, string code, string name)
        {
            this.Id     = id;
            this.Code   = code;
            this.Name   = name;
        }
        public int              Id          { get; private set; }
        public string           Code        { get; private set; }
        public string           Name        { get; private set; }
    }

    [Table("OrgTree")]
    public class OrgTreeBase : IID
    {
        [Key, Column("ID")]
        public int    Id           { get; set; }

        [Required, MaxLength(200)]
        public string Name         { get; set; }

        [Required, MaxLength(50)]
        public string OrgTypeCode  { get; set; }

        [Required, MaxLength(50)]
        public string Code         { get; set; }

        public int?  ParentID      { get; set; }
    }
}