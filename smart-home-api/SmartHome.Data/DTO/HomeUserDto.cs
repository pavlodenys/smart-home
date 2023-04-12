﻿namespace SmartHome.Data.DTO
{
    public class HomeUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string UserName
        {
            get; set;
        }
    }
}