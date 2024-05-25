using AutoMapper;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Helpers;
using Maintainify.Core.ModelView.OrderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Maintainify.BusinessLayer.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderModel>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OrderStatus.ToString()));

            CreateMap<OrderModel, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Parse(src.OrderDate)))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => Enum.Parse<OrderStatus>(src.Status)));
        }
    }
}
