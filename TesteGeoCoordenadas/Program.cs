using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using static Program;

class Program
{
    static void Main()
    {
        //var foto = new Ativo { Nome = "FOTO.PARA.LOCALIZAR", Easting = -21.12325652492897, Northing = -42.926571779040394 };
        //var foto2 = new Ativo { Nome = "FOTO.PARA.LOCALIZAR", Easting = -21.12309116259506, Northing = -42.92655264255299 };
        //var dist = HaversineDistance(foto.Easting, foto.Northing, foto2.Easting, foto2.Northing);

        var ativos = ConverterBaseAtivos();

        // Ativo de referência (foto)
        var foto = new Ativo { Nome = "FOTO.PARA.LOCALIZAR", Easting = -21.14712336914549, Northing = -42.94007475167339 };

        // Buscar ativos que estejam a exatamente 50 metros de distância
        var ativosProximos = BuscarAtivosProximos(ativos, foto, 50);

        Console.WriteLine($"Ativos a exatamente 50 metros de {foto.Nome}:");
        foreach (var ativo in ativosProximos)
        {
            Console.WriteLine($"- {ativo.Nome} ({ativo.Easting}, {ativo.Northing})");
        }
    }

    public class Ativo
    {
        public string Nome { get; set; } = string.Empty;
        public double Easting { get; set; }  // Coordenada X (E)
        public double Northing { get; set; } // Coordenada Y (N)
    }

    static List<Ativo> ConverterBaseAtivos()
    {
        var baseAtivos = new List<Ativo>
        {
            new() { Nome = "POSTE.0368520", Easting = 713930.79, Northing = 7660197.25 },
            new() { Nome = "POSTE.0368521", Easting = 713913.74, Northing = 7660161.9 },
            new() { Nome = "POSTE.0368522", Easting = 713687.64, Northing = 7660105.01 },
            new() { Nome = "POSTE.0368523", Easting = 713628.27, Northing = 7660099.75 },
            new() { Nome = "POSTE.0368524", Easting = 713598.21, Northing = 7660146.73 },
            new() { Nome = "POSTE.0368525", Easting = 713543.81, Northing = 7660274.52 },
            new() { Nome = "POSTE.0368526", Easting = 713645.45, Northing = 7660436.98 },
            new() { Nome = "POSTE.0368527", Easting = 713633.73, Northing = 7660429.54 },
            new() { Nome = "POSTE.0368528", Easting = 713546.58, Northing = 7660354.87 },
            new() { Nome = "POSTE.0054004-7", Easting = 721090.1, Northing = 7666504.51 },
            new() { Nome = "POSTE.0054006-2", Easting = 723395.65, Northing = 7662131.8 },
            new() { Nome = "POSTE.0054008-8", Easting = 720229.37, Northing = 7663086.71 },
            new() { Nome = "POSTE.0051984-3", Easting = 715021.31, Northing = 7660806.48 },
            new() { Nome = "POSTE.0051986-8", Easting = 715051.8, Northing = 7660820.63 },
            new() { Nome = "POSTE.0051988-4", Easting = 715036.69, Northing = 7660810.37 },
            new() { Nome = "POSTE.0051990-0", Easting = 714963.95, Northing = 7660810.35 },
            new() { Nome = "POSTE.0051992-6", Easting = 714938.19, Northing = 7660798.18 },
            new() { Nome = "POSTE.0051994-2", Easting = 714916.98, Northing = 7660782.18 },
            new() { Nome = "POSTE.0051996-7", Easting = 715074.81, Northing = 7660840.52 },
            new() { Nome = "POSTE.0051998-3", Easting = 714994.43, Northing = 7660807.01 },
            new() { Nome = "POSTE.0052000-7", Easting = 714754.49, Northing = 7660719.05 },
            new() { Nome = "POSTE.0052002-3", Easting = 715346.23, Northing = 7662803.53 }
        };

        var utm = ProjectedCoordinateSystem.WGS84_UTM(23, false);
        var wgs84 = GeographicCoordinateSystem.WGS84;
        var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(utm, wgs84);

        double[] latLon = [];
        foreach (var ativo in baseAtivos)
        {
            // Verifica se os valores estão em centímetros (CM) e converte para metros (M)
            if (ativo.Easting > 10000000 || ativo.Northing > 10000000)
            {
                latLon = transform.MathTransform.Transform(new double[] { ativo.Easting/100.0, ativo.Northing/100.0 });
            }
            else
            {
                latLon = transform.MathTransform.Transform(new double[] { ativo.Easting, ativo.Northing });
            }
            ativo.Easting = latLon[1];
            ativo.Northing = latLon[0];
        }

        return baseAtivos;
    }

    static List<Ativo> BuscarAtivosProximos(List<Ativo> ativos, Ativo foto, double raioMetros)
    {
        var newAtivos = new List<Ativo> { };

        foreach (var ativo in ativos) 
        {
            var dist = HaversineDistance(ativo.Easting, ativo.Northing, foto.Easting, foto.Northing);

            if (dist <= raioMetros)
            {
                newAtivos.Add(new() { Nome = ativo.Nome, Easting = ativo.Easting, Northing = ativo.Northing });
            }
        }

        return newAtivos;
    }

    public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadius = 6371000; // Raio da Terra em metros

        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c; // Retorna a distância em metros
    }

    static double ToRadians(double angle) => angle * (Math.PI / 180);
}
