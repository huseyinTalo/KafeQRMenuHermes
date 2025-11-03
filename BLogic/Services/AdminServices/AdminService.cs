using BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.AdminRepositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IAdminRepository adminRepository,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminService> logger)
        {
            _adminRepository = adminRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IResult> CreateAsync(AdminCreateDTO adminCreateDto)
        {
            _logger.LogInformation("Admin oluşturma işlemi başlatıldı. Email: {Email}", adminCreateDto.Email);
            try
            {
                // Validate password
                if (string.IsNullOrWhiteSpace(adminCreateDto.Password))
                {
                    _logger.LogWarning("Admin oluşturulamadı. Şifre boş olamaz. Email: {Email}", adminCreateDto.Email);
                    return new ErrorResult("Şifre boş olamaz.");
                }

                // Check if email already exists
                _logger.LogDebug("Email kontrolü yapılıyor: {Email}", adminCreateDto.Email);
                var existingUser = await _userManager.FindByEmailAsync(adminCreateDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Admin oluşturulamadı. Email zaten mevcut: {Email}", adminCreateDto.Email);
                    return new ErrorResult("Bu email adresi zaten kullanılıyor.");
                }

                // Ensure Admin role exists
                var adminRoleName = Roles.Admin.ToString();
                if (!await _roleManager.RoleExistsAsync(adminRoleName))
                {
                    _logger.LogWarning("Admin rolü bulunamadı, oluşturuluyor: {RoleName}", adminRoleName);
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = adminRoleName,
                        NormalizedName = adminRoleName.ToUpperInvariant()
                    });

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("Admin rolü oluşturulamadı. Hatalar: {Errors}", errors);
                        return new ErrorResult($"Admin rolü oluşturulamadı: {errors}");
                    }
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor.");
                var strategy = await _adminRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _adminRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        // Create Identity User with password
                        _logger.LogDebug("Identity kullanıcısı oluşturuluyor. Email: {Email}", adminCreateDto.Email);
                        var identityUser = new IdentityUser
                        {
                            UserName = adminCreateDto.Email,
                            Email = adminCreateDto.Email,
                            NormalizedEmail = adminCreateDto.Email.ToUpperInvariant(),
                            NormalizedUserName = adminCreateDto.Email.ToUpperInvariant(),
                            EmailConfirmed = true
                        };

                        var identityResult = await _userManager.CreateAsync(identityUser, adminCreateDto.Password);
                        if (!identityResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                            _logger.LogError("Identity kullanıcısı oluşturulamadı. Email: {Email}, Hatalar: {Errors}",
                                adminCreateDto.Email, errors);
                            return new ErrorResult($"Kullanıcı oluşturulamadı: {errors}");
                        }

                        _logger.LogInformation("Identity kullanıcısı başarıyla oluşturuldu. IdentityId: {IdentityId}, Email: {Email}",
                            identityUser.Id, adminCreateDto.Email);

                        // Assign Admin role to user
                        _logger.LogDebug("Kullanıcıya Admin rolü atanıyor. IdentityId: {IdentityId}", identityUser.Id);
                        var roleAssignResult = await _userManager.AddToRoleAsync(identityUser, adminRoleName);
                        if (!roleAssignResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            var errors = string.Join(", ", roleAssignResult.Errors.Select(e => e.Description));
                            _logger.LogError("Kullanıcıya Admin rolü atanamadı. IdentityId: {IdentityId}, Hatalar: {Errors}",
                                identityUser.Id, errors);
                            return new ErrorResult($"Kullanıcıya Admin rolü atanamadı: {errors}");
                        }

                        _logger.LogInformation("Admin rolü başarıyla atandı. IdentityId: {IdentityId}", identityUser.Id);

                        // Create Admin entity (without password)
                        _logger.LogDebug("Admin entity oluşturuluyor. IdentityId: {IdentityId}", identityUser.Id);
                        var admin = adminCreateDto.Adapt<Admin>();
                        admin.IdentityId = identityUser.Id;

                        await _adminRepository.AddAsync(admin);
                        int effectedRows = await _adminRepository.SaveChangeAsync();
                        if (effectedRows <= 0)
                        {
                            await transaction.RollbackAsync();
                            return new ErrorResult("Admin oluşturma sırasında hata oluştu.");
                        }

                            _logger.LogInformation("Admin entity başarıyla oluşturuldu. AdminId: {AdminId}, IdentityId: {IdentityId}",
                                admin.Id, identityUser.Id);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Admin başarıyla oluşturuldu. Email: {Email}",
                            adminCreateDto.Email);

                        return new SuccessResult("Admin başarıyla oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Admin oluşturma sırasında hata oluştu. Transaction rollback yapıldı. Email: {Email}",
                                adminCreateDto.Email);
                        }
                        else
                        {
                            _logger.LogError(ex, "Admin oluşturma sırasında hata oluştu. Email: {Email}", adminCreateDto.Email);
                        }

                        return new ErrorResult($"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin oluşturma işlemi başarısız oldu. Email: {Email}", adminCreateDto.Email);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(AdminDTO adminDto)
        {
            _logger.LogInformation("Admin silme işlemi başlatıldı. AdminId: {AdminId}, Email: {Email}",
                adminDto.Id, adminDto.Email);

            try
            {
                // Get admin entity
                _logger.LogDebug("Admin entity getiriliyor. AdminId: {AdminId}", adminDto.Id);
                var admin = await _adminRepository.GetById(adminDto.Id);
                if (admin == null)
                {
                    _logger.LogWarning("Admin bulunamadı. AdminId: {AdminId}", adminDto.Id);
                    return new ErrorResult("Admin bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. AdminId: {AdminId}", adminDto.Id);
                var strategy = await _adminRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _adminRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        // Delete Identity User (this also deletes the password hash and role assignments)
                        _logger.LogDebug("Identity kullanıcısı siliniyor. IdentityId: {IdentityId}", admin.IdentityId);
                        var identityUser = await _userManager.FindByIdAsync(admin.IdentityId);
                        if (identityUser != null)
                        {
                            // Check if user has Admin role
                            var isInAdminRole = await _userManager.IsInRoleAsync(identityUser, Roles.Admin.ToString());
                            if (isInAdminRole)
                            {
                                _logger.LogDebug("Kullanıcıdan Admin rolü kaldırılıyor. IdentityId: {IdentityId}", admin.IdentityId);
                                var removeRoleResult = await _userManager.RemoveFromRoleAsync(identityUser, Roles.Admin.ToString());
                                if (!removeRoleResult.Succeeded)
                                {
                                    _logger.LogWarning("Admin rolü kaldırılamadı, silme işlemine devam ediliyor. IdentityId: {IdentityId}",
                                        admin.IdentityId);
                                }
                            }

                            var identityResult = await _userManager.DeleteAsync(identityUser);
                            if (!identityResult.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                                _logger.LogError("Identity kullanıcısı silinemedi. IdentityId: {IdentityId}, Hatalar: {Errors}",
                                    admin.IdentityId, errors);
                                return new ErrorResult($"Identity kullanıcısı silinemedi: {errors}");
                            }
                            _logger.LogInformation("Identity kullanıcısı başarıyla silindi. IdentityId: {IdentityId}",
                                admin.IdentityId);
                        }
                        else
                        {
                            _logger.LogWarning("Identity kullanıcısı bulunamadı. IdentityId: {IdentityId}", admin.IdentityId);
                        }

                        // Delete Admin (soft delete will happen automatically in DbContext)
                        _logger.LogDebug("Admin entity siliniyor (soft delete). AdminId: {AdminId}", adminDto.Id);
                        await _adminRepository.DeleteAsync(admin);
                        await _adminRepository.SaveChangeAsync();

                        _logger.LogInformation("Admin entity başarıyla silindi. AdminId: {AdminId}", adminDto.Id);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Admin başarıyla silindi. AdminId: {AdminId}, Email: {Email}",
                            adminDto.Id, adminDto.Email);

                        return new SuccessResult("Admin başarıyla silindi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Admin silme sırasında hata oluştu. Transaction rollback yapıldı. AdminId: {AdminId}",
                                adminDto.Id);
                        }
                        else
                        {
                            _logger.LogError(ex, "Admin silme sırasında hata oluştu. AdminId: {AdminId}", adminDto.Id);
                        }

                        return new ErrorResult($"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin silme işlemi başarısız oldu. AdminId: {AdminId}", adminDto.Id);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<AdminListDTO>>> GetAllAsync()
        {
            _logger.LogInformation("Tüm adminler listeleniyor.");

            try
            {
                var admins = await _adminRepository.GetAllAsync(tracking: false);

                if (admins == null || !admins.Any())
                {
                    _logger.LogInformation("Hiç admin bulunamadı.");
                    return new SuccessDataResult<List<AdminListDTO>>(
                        new List<AdminListDTO>(),
                        "Hiç admin bulunamadı."
                    );
                }

                var adminListDtos = admins.Adapt<List<AdminListDTO>>();

                _logger.LogInformation("Adminler başarıyla listelendi. Toplam: {Count}", adminListDtos.Count);

                return new SuccessDataResult<List<AdminListDTO>>(
                    adminListDtos,
                    "Adminler başarıyla listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adminleri listelerken hata oluştu.");
                return new ErrorDataResult<List<AdminListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id)
        {
            _logger.LogInformation("Admin getiriliyor. AdminId: {AdminId}", Id);

            try
            {
                var admin = await _adminRepository.GetById(Id, tracking: false);

                if (admin == null)
                {
                    _logger.LogWarning("Admin bulunamadı. AdminId: {AdminId}", Id);
                    return new ErrorDataResult<AdminDTO>(
                        null,
                        "Admin bulunamadı."
                    );
                }

                var adminDto = admin.Adapt<AdminDTO>();

                _logger.LogInformation("Admin başarıyla getirildi. AdminId: {AdminId}, Email: {Email}",
                    Id, adminDto.Email);

                return new SuccessDataResult<AdminDTO>(
                    adminDto,
                    "Admin başarıyla getirildi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin getirilirken hata oluştu. AdminId: {AdminId}", Id);
                return new ErrorDataResult<AdminDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> UpdateAsync(AdminUpdateDTO adminUpdateDto)
        {
            _logger.LogInformation("Admin güncelleme işlemi başlatıldı. AdminId: {AdminId}, Email: {Email}",
                adminUpdateDto.Id, adminUpdateDto.Email);

            try
            {
                // Get existing admin
                _logger.LogDebug("Mevcut admin getiriliyor. AdminId: {AdminId}", adminUpdateDto.Id);
                var admin = await _adminRepository.GetById(adminUpdateDto.Id);
                if (admin == null)
                {
                    _logger.LogWarning("Admin bulunamadı. AdminId: {AdminId}", adminUpdateDto.Id);
                    return new ErrorResult("Admin bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. AdminId: {AdminId}", adminUpdateDto.Id);
                var strategy = await _adminRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _adminRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        // Update Identity User
                        _logger.LogDebug("Identity kullanıcısı getiriliyor. IdentityId: {IdentityId}", admin.IdentityId);
                        var identityUser = await _userManager.FindByIdAsync(admin.IdentityId);
                        if (identityUser == null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("Identity kullanıcısı bulunamadı. IdentityId: {IdentityId}, AdminId: {AdminId}",
                                admin.IdentityId, adminUpdateDto.Id);
                            return new ErrorResult("Identity kullanıcısı bulunamadı.");
                        }

                        // Update email if changed
                        if (identityUser.Email != adminUpdateDto.Email)
                        {
                            _logger.LogInformation("Email değişikliği tespit edildi. Eski: {OldEmail}, Yeni: {NewEmail}",
                                identityUser.Email, adminUpdateDto.Email);

                            // Check if new email already exists
                            var existingUser = await _userManager.FindByEmailAsync(adminUpdateDto.Email);
                            if (existingUser != null && existingUser.Id != identityUser.Id)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogWarning("Email değiştirilemedi. Email başka bir kullanıcı tarafından kullanılıyor: {Email}",
                                    adminUpdateDto.Email);
                                return new ErrorResult("Bu email adresi başka bir kullanıcı tarafından kullanılıyor.");
                            }

                            identityUser.Email = adminUpdateDto.Email;
                            identityUser.UserName = adminUpdateDto.Email;
                            identityUser.NormalizedEmail = adminUpdateDto.Email.ToUpperInvariant();
                            identityUser.NormalizedUserName = adminUpdateDto.Email.ToUpperInvariant();

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
                        if (!string.IsNullOrWhiteSpace(adminUpdateDto.Password))
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

                            var addPasswordResult = await _userManager.AddPasswordAsync(identityUser, adminUpdateDto.Password);
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

                        // Update Admin entity (without password)
                        _logger.LogDebug("Admin entity güncelleniyor. AdminId: {AdminId}", adminUpdateDto.Id);
                        admin.FirstName = adminUpdateDto.FirstName;
                        admin.LastName = adminUpdateDto.LastName;
                        admin.Email = adminUpdateDto.Email;

                        await _adminRepository.UpdateAsync(admin);
                        await _adminRepository.SaveChangeAsync();

                        _logger.LogInformation("Admin entity başarıyla güncellendi. AdminId: {AdminId}", adminUpdateDto.Id);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Admin başarıyla güncellendi. AdminId: {AdminId}, Email: {Email}",
                            adminUpdateDto.Id, adminUpdateDto.Email);

                        return new SuccessResult("Admin başarıyla güncellendi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Admin güncelleme sırasında hata oluştu. Transaction rollback yapıldı. AdminId: {AdminId}",
                                adminUpdateDto.Id);
                        }
                        else
                        {
                            _logger.LogError(ex, "Admin güncelleme sırasında hata oluştu. AdminId: {AdminId}", adminUpdateDto.Id);
                        }

                        return new ErrorResult($"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin güncelleme işlemi başarısız oldu. AdminId: {AdminId}", adminUpdateDto.Id);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }
    }
}