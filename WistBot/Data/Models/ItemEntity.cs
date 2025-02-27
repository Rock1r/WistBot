﻿using WistBot.Enums;

namespace WistBot.Data.Models
{
    public class ItemEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Media { get; set; } = string.Empty;
        public MediaTypes MediaType { get; set; } = MediaTypes.Photo;
        public string PerformerName { get; set; } = string.Empty;
        public State CurrentState { get; set; } = State.Free;
        public Guid ListId { get; set; }
        public WishListEntity? List { get; set; }
        public long OwnerId { get; set; }
        public UserEntity? Owner { get; set; }
    }
}
