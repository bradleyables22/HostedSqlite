
namespace Server.Services
{
	public class SecretRotator : BackgroundService
	{
		private readonly ServerSettings _settings;

		public SecretRotator(ServerSettings settings)
		{
			_settings = settings;
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var enabled = IsRotationEnabled();
			if (!enabled)
				return;

			var interval = GetIntervalHours();

			while (!stoppingToken.IsCancellationRequested) 
			{
				await Task.Delay(interval, stoppingToken);

				try
				{
					await _settings.ChangeSecretAsync();
				}
				catch (Exception)
				{
					continue;
				}
			}
		}
		private static bool IsRotationEnabled()
		{
			var value = Environment.GetEnvironmentVariable("SECRET_ROTATE_ENABLED");
			return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
		}

		private static TimeSpan GetIntervalHours()
		{
			var env = Environment.GetEnvironmentVariable("SECRET_ROTATE_INTERVAL_HOURS");
			if (int.TryParse(env, out var hours) && hours > 0)
				return TimeSpan.FromHours(hours);

			return TimeSpan.FromHours(24);
		}
	}
}
