using AutoMapper;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using SyriaNTMP.Dto;
using SyriaNTMP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SyriaNTMP.Services
{
    [Authorize]
    public class ReservationService : ApplicationService
    {
        private readonly IRepository<Reservations> _reservationsRepository;

        public ReservationService(IRepository<Reservations> reservationsRepository)
        {
            _reservationsRepository = reservationsRepository;
        }

        public async Task<PagedResultDto<ReservationsDto>> Get(ReservationsSearchCriteria searchCriteria)
        {
            var skip = searchCriteria.SkipCount;
            var take = searchCriteria.MaxResultCount;


            Expression<Func<Reservations, bool>> predicate = x => true;
            var data = await _reservationsRepository.GetPagedListAsync(
                skipCount: skip,
                maxResultCount: take,
                sorting: "Id ASC"
            );
            var count = await _reservationsRepository.CountAsync(predicate);

            var result = new PagedResultDto<ReservationsDto>();
            var mappedResultItems = ObjectMapper.Map<List<Reservations>, List<ReservationsDto>>(data);
            result.Items = mappedResultItems;
            result.TotalCount = count;

            return result;
        }
        public async Task<ReservationsDto> GetById(int id)
        {
            var data = await _reservationsRepository.GetAsync(x => x.Id == id);
            return ObjectMapper.Map<Reservations, ReservationsDto>(data);
        }

        public async Task<ReservationsDto> CreateAsync(ReservationsDto dto)
        {
            var entity = ObjectMapper.Map<ReservationsDto, Reservations>(dto);
            await _reservationsRepository.InsertAsync(entity);
            return ObjectMapper.Map<Reservations, ReservationsDto>(entity);
        }

        public async Task<ReservationsDto> UpdateAsync(ReservationsDto dto)
        {
            var entity = await _reservationsRepository.GetAsync(x => x.Id == dto.Id);
            entity = ObjectMapper.Map(dto, entity);
            await _reservationsRepository.UpdateAsync(entity);
            return ObjectMapper.Map<Reservations, ReservationsDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _reservationsRepository.DeleteDirectAsync(x => x.Id == id);
            return true;
        }
    }
}

