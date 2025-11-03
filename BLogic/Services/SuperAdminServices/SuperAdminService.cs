using BLogic.DTOs.SuperAdminDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.SuperAdminServices
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ISuperAdminRepository _superAdminRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(
            ISuperAdminRepository superAdminRepository,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SuperAdminService> logger)
        {
            _superAdminRepository = superAdminRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IResult> CreateAsync(SuperAdminCreateDTO superAdminCreateDto)
        {
            _logger.LogInformation("SuperAdmin oluşturma işlemi başlatıldı. Email: {Email}", superAdminCreateDto.Email);

            IDbContextTransaction transaction = null;
            try
            {
                // Validate password
                if (string.IsNullOrWhiteSpace(superAdminCreateDto.Password))
                {
                    _logger.LogWarning("SuperAdmin oluşturulamadı. Şifre boş olamaz. Email: {Email}", superAdminCreateDto.Email);
                    return new ErrorResult("Şifre boş olamaz.");
                }

                // Check if email already exists
                _logger.LogDebug("Email kontrolü yapılıyor: {Email}", superAdminCreateDto.Email);
                var existingUser = await _userManager.FindByEmailAsync(superAdminCreateDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("SuperAdmin oluşturulamadı. Email zaten mevcut: {Email}", superAdminCreateDto.Email);
                    return new ErrorResult("Bu email adresi zaten kullanılıyor.");
                }

                // Ensure SuperAdmin role exists
                var superAdminRoleName = Roles.SuperAdmin.ToString();
                if (!await _roleManager.RoleExistsAsync(superAdminRoleName))
                {
                    _logger.LogWarning("SuperAdmin rolü bulunamadı, oluşturuluyor: {RoleName}", superAdminRoleName);
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = superAdminRoleName,
                        NormalizedName = superAdminRoleName.ToUpperInvariant()
                    });

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("SuperAdmin rolü oluşturulamadı. Hatalar: {Errors}", errors);
                        return new ErrorResult($"SuperAdmin rolü oluşturulamadı: {errors}");
                    }
                }

                // Begin transaction
                _logger.LogDebug("Transaction başlatılıyor.");
                transaction = await _superAdminRepository.BeginTransactionAsync();

                // Create Identity User with password
                _logger.LogDebug("Identity kullanıcısı oluşturuluyor. Email: {Email}", superAdminCreateDto.Email);
                var identityUser = new IdentityUser
                {
                    UserName = superAdminCreateDto.Email,
                    Email = superAdminCreateDto.Email,
                    NormalizedEmail = superAdminCreateDto.Email.ToUpperInvariant(),
                    NormalizedUserName = superAdminCreateDto.Email.ToUpperInvariant(),
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(identityUser, superAdminCreateDto.Password);
                if (!identityResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogError("Identity kullanıcısı oluşturulamadı. Email: {Email}, Hatalar: {Errors}",
                        superAdminCreateDto.Email, errors);
                    return new ErrorResult($"Kullanıcı oluşturulamadı: {errors}");
                }

                _logger.LogInformation("Identity kullanıcısı başarıyla oluşturuldu. IdentityId: {IdentityId}, Email: {Email}",
                    identityUser.Id, superAdminCreateDto.Email);

                // Assign SuperAdmin role to user
                _logger.LogDebug("Kullanıcıya SuperAdmin rolü atanıyor. IdentityId: {IdentityId}", identityUser.Id);
                var roleAssignResult = await _userManager.AddToRoleAsync(identityUser, superAdminRoleName);
                if (!roleAssignResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", roleAssignResult.Errors.Select(e => e.Description));
                    _logger.LogError("Kullanıcıya SuperAdmin rolü atanamadı. IdentityId: {IdentityId}, Hatalar: {Errors}",
                        identityUser.Id, errors);
                    return new ErrorResult($"Kullanıcıya SuperAdmin rolü atanamadı: {errors}");
                }

                _logger.LogInformation("SuperAdmin rolü başarıyla atandı. IdentityId: {IdentityId}", identityUser.Id);

                // Create SuperAdmin entity (without password)
                _logger.LogDebug("SuperAdmin entity oluşturuluyor. IdentityId: {IdentityId}", identityUser.Id);
                var superAdmin = superAdminCreateDto.Adapt<SuperAdmin>();
                superAdmin.IdentityId = identityUser.Id;

                await _superAdminRepository.AddAsync(superAdmin);
                await _superAdminRepository.SaveChangeAsync();

                _logger.LogInformation("SuperAdmin entity başarıyla oluşturuldu. SuperAdminId: {SuperAdminId}, IdentityId: {IdentityId}",
                    superAdmin.Id, identityUser.Id);

                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction commit edildi. SuperAdmin başarıyla oluşturuldu. Email: {Email}",
                    superAdminCreateDto.Email);

                return new SuccessResult("SuperAdmin başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "SuperAdmin oluşturma sırasında hata oluştu. Transaction rollback yapıldı. Email: {Email}",
                        superAdminCreateDto.Email);
                }
                else
                {
                    _logger.LogError(ex, "SuperAdmin oluşturma sırasında hata oluştu. Email: {Email}", superAdminCreateDto.Email);
                }

                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task<IResult> DeleteAsync(SuperAdminDTO superAdminDto)
        {
            _logger.LogInformation("SuperAdmin silme işlemi başlatıldı. SuperAdminId: {SuperAdminId}, Email: {Email}",
                superAdminDto.SuperAdminId, superAdminDto.Email);

            IDbContextTransaction transaction = null;
            try
            {
                // Get superAdmin entity
                _logger.LogDebug("SuperAdmin entity getiriliyor. SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);
                var superAdmin = await _superAdminRepository.GetById(superAdminDto.SuperAdminId);
                if (superAdmin == null)
                {
                    _logger.LogWarning("SuperAdmin bulunamadı. SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);
                    return new ErrorResult("SuperAdmin bulunamadı.");
                }

                // Begin transaction
                _logger.LogDebug("Transaction başlatılıyor. SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);
                transaction = await _superAdminRepository.BeginTransactionAsync();

                // Delete Identity User (this also deletes the password hash and role assignments)
                _logger.LogDebug("Identity kullanıcısı siliniyor. IdentityId: {IdentityId}", superAdmin.IdentityId);
                var identityUser = await _userManager.FindByIdAsync(superAdmin.IdentityId);
                if (identityUser != null)
                {
                    // Check if user has SuperAdmin role
                    var isInSuperAdminRole = await _userManager.IsInRoleAsync(identityUser, Roles.SuperAdmin.ToString());
                    if (isInSuperAdminRole)
                    {
                        _logger.LogDebug("Kullanıcıdan SuperAdmin rolü kaldırılıyor. IdentityId: {IdentityId}", superAdmin.IdentityId);
                        var removeRoleResult = await _userManager.RemoveFromRoleAsync(identityUser, Roles.SuperAdmin.ToString());
                        if (!removeRoleResult.Succeeded)
                        {
                            _logger.LogWarning("SuperAdmin rolü kaldırılamadı, silme işlemine devam ediliyor. IdentityId: {IdentityId}",
                                superAdmin.IdentityId);
                        }
                    }

                    var identityResult = await _userManager.DeleteAsync(identityUser);
                    if (!identityResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                        _logger.LogError("Identity kullanıcısı silinemedi. IdentityId: {IdentityId}, Hatalar: {Errors}",
                            superAdmin.IdentityId, errors);
                        return new ErrorResult($"Identity kullanıcısı silinemedi: {errors}");
                    }
                    _logger.LogInformation("Identity kullanıcısı başarıyla silindi. IdentityId: {IdentityId}",
                        superAdmin.IdentityId);
                }
                else
                {
                    _logger.LogWarning("Identity kullanıcısı bulunamadı. IdentityId: {IdentityId}", superAdmin.IdentityId);
                }

                // Delete SuperAdmin (soft delete will happen automatically in DbContext)
                _logger.LogDebug("SuperAdmin entity siliniyor (soft delete). SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);
                await _superAdminRepository.DeleteAsync(superAdmin);
                await _superAdminRepository.SaveChangeAsync();

                _logger.LogInformation("SuperAdmin entity başarıyla silindi. SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);

                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction commit edildi. SuperAdmin başarıyla silindi. SuperAdminId: {SuperAdminId}, Email: {Email}",
                    superAdminDto.SuperAdminId, superAdminDto.Email);

                return new SuccessResult("SuperAdmin başarıyla silindi.");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "SuperAdmin silme sırasında hata oluştu. Transaction rollback yapıldı. SuperAdminId: {SuperAdminId}",
                        superAdminDto.SuperAdminId);
                }
                else
                {
                    _logger.LogError(ex, "SuperAdmin silme sırasında hata oluştu. SuperAdminId: {SuperAdminId}", superAdminDto.SuperAdminId);
                }

                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task<IDataResult<List<SuperAdminListDTO>>> GetAllAsync()
        {
            _logger.LogInformation("Tüm SuperAdminler listeleniyor.");

            try
            {
                var superAdmins = await _superAdminRepository.GetAllAsync(tracking: false);

                if (superAdmins == null || !superAdmins.Any())
                {
                    _logger.LogInformation("Hiç SuperAdmin bulunamadı.");
                    return new SuccessDataResult<List<SuperAdminListDTO>>(
                        new List<SuperAdminListDTO>(),
                        "Hiç SuperAdmin bulunamadı."
                    );
                }

                var superAdminListDtos = superAdmins.Adapt<List<SuperAdminListDTO>>();

                _logger.LogInformation("SuperAdminler başarıyla listelendi. Toplam: {Count}", superAdminListDtos.Count);

                return new SuccessDataResult<List<SuperAdminListDTO>>(
                    superAdminListDtos,
                    "SuperAdminler başarıyla listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SuperAdminleri listelerken hata oluştu.");
                return new ErrorDataResult<List<SuperAdminListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<SuperAdminDTO>> GetByIdAsync(Guid Id)
        {
            _logger.LogInformation("SuperAdmin getiriliyor. SuperAdminId: {SuperAdminId}", Id);

            try
            {
                var superAdmin = await _superAdminRepository.GetById(Id, tracking: false);

                if (superAdmin == null)
                {
                    _logger.LogWarning("SuperAdmin bulunamadı. SuperAdminId: {SuperAdminId}", Id);
                    return new ErrorDataResult<SuperAdminDTO>(
                        null,
                        "SuperAdmin bulunamadı."
                    );
                }

                var superAdminDto = superAdmin.Adapt<SuperAdminDTO>();

                _logger.LogInformation("SuperAdmin başarıyla getirildi. SuperAdminId: {SuperAdminId}, Email: {Email}",
                    Id, superAdminDto.Email);

                return new SuccessDataResult<SuperAdminDTO>(
                    superAdminDto,
                    "SuperAdmin başarıyla getirildi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SuperAdmin getirilirken hata oluştu. SuperAdminId: {SuperAdminId}", Id);
                return new ErrorDataResult<SuperAdminDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> UpdateAsync(SuperAdminUpdateDTO superAdminUpdateDto)
        {
            _logger.LogInformation("SuperAdmin güncelleme işlemi başlatıldı. SuperAdminId: {SuperAdminId}, Email: {Email}",
                superAdminUpdateDto.SuperAdminId, superAdminUpdateDto.Email);

            IDbContextTransaction transaction = null;
            try
            {
                // Get existing superAdmin
                _logger.LogDebug("Mevcut SuperAdmin getiriliyor. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);
                var superAdmin = await _superAdminRepository.GetById(superAdminUpdateDto.SuperAdminId);
                if (superAdmin == null)
                {
                    _logger.LogWarning("SuperAdmin bulunamadı. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);
                    return new ErrorResult("SuperAdmin bulunamadı.");
                }

                // Begin transaction
                _logger.LogDebug("Transaction başlatılıyor. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);
                transaction = await _superAdminRepository.BeginTransactionAsync();

                // Update Identity User
                _logger.LogDebug("Identity kullanıcısı getiriliyor. IdentityId: {IdentityId}", superAdmin.IdentityId);
                var identityUser = await _userManager.FindByIdAsync(superAdmin.IdentityId);
                if (identityUser == null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Identity kullanıcısı bulunamadı. IdentityId: {IdentityId}, SuperAdminId: {SuperAdminId}",
                        superAdmin.IdentityId, superAdminUpdateDto.SuperAdminId);
                    return new ErrorResult("Identity kullanıcısı bulunamadı.");
                }

                // Update email if changed
                if (identityUser.Email != superAdminUpdateDto.Email)
                {
                    _logger.LogInformation("Email değişikliği tespit edildi. Eski: {OldEmail}, Yeni: {NewEmail}",
                        identityUser.Email, superAdminUpdateDto.Email);

                    // Check if new email already exists
                    var existingUser = await _userManager.FindByEmailAsync(superAdminUpdateDto.Email);
                    if (existingUser != null && existingUser.Id != identityUser.Id)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Email değiştirilemedi. Email başka bir kullanıcı tarafından kullanılıyor: {Email}",
                            superAdminUpdateDto.Email);
                        return new ErrorResult("Bu email adresi başka bir kullanıcı tarafından kullanılıyor.");
                    }

                    identityUser.Email = superAdminUpdateDto.Email;
                    identityUser.UserName = superAdminUpdateDto.Email;
                    identityUser.NormalizedEmail = superAdminUpdateDto.Email.ToUpperInvariant();
                    identityUser.NormalizedUserName = superAdminUpdateDto.Email.ToUpperInvariant();

                    _logger.LogDebug("Identity kullanıcısı güncelleniyor. IdentityId: {IdentityId}", identityUser.Id);
                    var identityResult = await _userManager.UpdateAsync(identityUser);
                    if (!identityResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                        _logger.LogError("Identity kullanıcısı güncellenemedi. IdentityId: {IdentityId}, Hatalar: {Errors}",
                            identityUser.Id, errors);
                        return new ErrorResult($"Identity kullanıcısı güncellenemedi: {errors}");
                    }

                    _logger.LogInformation("Identity kullanıcısı email bilgisi başarıyla güncellendi. IdentityId: {IdentityId}",
                        identityUser.Id);
                }

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(superAdminUpdateDto.Password))
                {
                    _logger.LogDebug("Şifre güncelleme işlemi başlatıldı. IdentityId: {IdentityId}", identityUser.Id);

                    // Remove old password and add new one
                    var removePasswordResult = await _userManager.RemovePasswordAsync(identityUser);
                    if (!removePasswordResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                        _logger.LogError("Eski şifre kaldırılamadı. IdentityId: {IdentityId}, Hatalar: {Errors}",
                            identityUser.Id, errors);
                        return new ErrorResult($"Şifre güncellenemedi: {errors}");
                    }

                    var addPasswordResult = await _userManager.AddPasswordAsync(identityUser, superAdminUpdateDto.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                        _logger.LogError("Yeni şifre eklenemedi. IdentityId: {IdentityId}, Hatalar: {Errors}",
                            identityUser.Id, errors);
                        return new ErrorResult($"Şifre güncellenemedi: {errors}");
                    }

                    _logger.LogInformation("Şifre başarıyla güncellendi. IdentityId: {IdentityId}", identityUser.Id);
                }

                // Update SuperAdmin entity (without password)
                _logger.LogDebug("SuperAdmin entity güncelleniyor. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);
                superAdmin.FirstName = superAdminUpdateDto.FirstName;
                superAdmin.LastName = superAdminUpdateDto.LastName;
                superAdmin.Email = superAdminUpdateDto.Email;

                await _superAdminRepository.UpdateAsync(superAdmin);
                await _superAdminRepository.SaveChangeAsync();

                _logger.LogInformation("SuperAdmin entity başarıyla güncellendi. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);

                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction commit edildi. SuperAdmin başarıyla güncellendi. SuperAdminId: {SuperAdminId}, Email: {Email}",
                    superAdminUpdateDto.SuperAdminId, superAdminUpdateDto.Email);

                return new SuccessResult("SuperAdmin başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "SuperAdmin güncelleme sırasında hata oluştu. Transaction rollback yapıldı. SuperAdminId: {SuperAdminId}",
                        superAdminUpdateDto.SuperAdminId);
                }
                else
                {
                    _logger.LogError(ex, "SuperAdmin güncelleme sırasında hata oluştu. SuperAdminId: {SuperAdminId}", superAdminUpdateDto.SuperAdminId);
                }

                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
            finally
            {
                transaction?.Dispose();
            }
        }
    }
}