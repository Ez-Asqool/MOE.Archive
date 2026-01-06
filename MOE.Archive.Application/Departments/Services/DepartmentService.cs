using AutoMapper;
using MOE.Archive.Application.Departments.DTOs;
using MOE.Archive.Domain.Entities;
using MOE.Archive.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Departments.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _departmentRepository = departmentRepository;   
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }

        public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentRequestDto requestDto, Guid? userId, CancellationToken cancellationToken)
        {
            var departmentEntity = _mapper.Map<Department>(requestDto);
            departmentEntity.CreatedBy = userId;

            await _departmentRepository.AddAsync(departmentEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var departmentResponseDto = _mapper.Map<DepartmentResponseDto>(departmentEntity);
            return departmentResponseDto;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var departmentEntities = await _departmentRepository.GetAllAsync(cancellationToken);

            var departmentResponseDtos = _mapper.Map<IEnumerable<DepartmentResponseDto>>(departmentEntities);
            return departmentResponseDtos;
        }

        public async Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var departmentEntity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (departmentEntity == null)
                throw new KeyNotFoundException("القسم غير موجود");

            var departmentResponseDto = _mapper.Map<DepartmentResponseDto>(departmentEntity);
            return departmentResponseDto;
        }

        public async Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentRequestDto requestDto, Guid? userId, CancellationToken cancellationToken = default)
        {
            var departmentEntity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (departmentEntity == null)
                throw new KeyNotFoundException("القسم غير موجود");

            _mapper.Map(requestDto, departmentEntity);
            departmentEntity.UpdatedBy = userId;    
            
            await _departmentRepository.UpdateAsync(departmentEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var departmentResponseDto = _mapper.Map<DepartmentResponseDto>(departmentEntity);
            return departmentResponseDto;
        }


        public async Task<DepartmentResponseDto> DeleteAsync(int id, Guid? userId, CancellationToken cancellationToken = default)
        {
            var departmentEntity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (departmentEntity == null)
                throw new KeyNotFoundException("القسم غير موجود");

            departmentEntity.UpdatedBy = userId;
            await _departmentRepository.DeleteAsync(departmentEntity, cancellationToken);   
            await _unitOfWork.SaveChangesAsync(cancellationToken);   

            var departmentResponseDto = _mapper.Map<DepartmentResponseDto>(departmentEntity);
            return departmentResponseDto;
        }


    }
}
