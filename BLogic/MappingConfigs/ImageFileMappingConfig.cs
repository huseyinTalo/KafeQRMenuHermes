using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;
using KafeQRMenu.Data.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class ImageFileMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Entity to ImageFileDTO mapping
            config.NewConfig<ImageFile, ImageFileDTO>()
                .Map(dest => dest.ImageId, src => src.Id).TwoWays();

            // Entity to ImageFileListDTO mapping
            config.NewConfig<ImageFile, ImageFileListDTO>()
                .Map(dest => dest.ImageId, src => src.Id).TwoWays();

            // ImageFileUpdateDTO to Entity mapping
            config.NewConfig<ImageFileUpdateDTO, ImageFile>()
                .Map(dest => dest.Id, src => src.ImageId).TwoWays();
        }
    }
}
