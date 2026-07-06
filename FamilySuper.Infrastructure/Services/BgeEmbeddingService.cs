using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FamilySuper.Infrastructure.Services;

public interface IEmbeddingService
{
    bool IsAvailable { get; }
    int Dimensions { get; }
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}

public class BgeEmbeddingService : IEmbeddingService
{
    private readonly ILogger<BgeEmbeddingService> _logger;
    private readonly string _modelPath;
    private readonly string _vocabPath;
    private readonly bool _enabled;
    private readonly int _maxTokens;

    private InferenceSession? _session;
    private Tokenizer? _tokenizer;
    private readonly object _initLock = new();
    private bool _initialized;

    public BgeEmbeddingService(IConfiguration configuration, ILogger<BgeEmbeddingService> logger)
    {
        _logger = logger;
        _modelPath = configuration["Embedding:ModelPath"] ?? "./models/embedding/bge-small-zh-v1.5.onnx";
        _vocabPath = configuration["Embedding:VocabPath"] ?? "./models/embedding/vocab.txt";
        _enabled = configuration.GetValue("Embedding:Enabled", false);
        _maxTokens = configuration.GetValue("Embedding:MaxTokens", 512);
    }

    public bool IsAvailable => _enabled && File.Exists(_modelPath) && File.Exists(_vocabPath);
    public int Dimensions => 512;

    private void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;

            if (!IsAvailable)
            {
                _logger.LogWarning("BGE 嵌入模型不可用: Enabled={Enabled}, ModelExists={ModelExists}, VocabExists={VocabExists}",
                    _enabled, File.Exists(_modelPath), File.Exists(_vocabPath));
                _initialized = true;
                return;
            }

            try
            {
                _logger.LogInformation("正在加载 BGE 嵌入模型: {ModelPath}", _modelPath);
                var options = new SessionOptions
                {
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                };
                _session = new InferenceSession(_modelPath, options);

                _tokenizer = BertTokenizer.Create(_vocabPath);
                _logger.LogInformation("BGE 嵌入模型加载完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BGE 嵌入模型加载失败");
                _session = null;
                _tokenizer = null;
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        if (_session is null || _tokenizer is null)
        {
            _logger.LogDebug("嵌入模型未加载,返回零向量");
            return new float[Dimensions];
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return new float[Dimensions];
        }

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tokenIds = _tokenizer.EncodeToIds(text).Take(_maxTokens).ToList();
            if (tokenIds.Count == 0) return new float[Dimensions];

            var tokens = new List<long> { 101L };
            tokens.AddRange(tokenIds.Where(t => t != 101 && t != 102).Select(t => (long)t));
            tokens.Add(102L);
            if (tokens.Count > _maxTokens) tokens = tokens.Take(_maxTokens).ToList();

            var seqLen = tokens.Count;
            var inputIds = new DenseTensor<long>(new[] { 1, seqLen });
            var attentionMask = new DenseTensor<long>(new[] { 1, seqLen });
            var tokenTypeIds = new DenseTensor<long>(new[] { 1, seqLen });

            for (var i = 0; i < seqLen; i++)
            {
                inputIds[0, i] = tokens[i];
                attentionMask[0, i] = 1;
                tokenTypeIds[0, i] = 0;
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
                NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>();

            var pooled = MeanPool(output, attentionMask, seqLen);
            return L2Normalize(pooled);
        }, cancellationToken);
    }

    private static float[] MeanPool(Tensor<float> lastHiddenState, Tensor<long> attentionMask, int seqLen)
    {
        var dims = lastHiddenState.Dimensions;
        var hiddenSize = dims[2];
        var pooled = new float[hiddenSize];

        for (var h = 0; h < hiddenSize; h++)
        {
            float sum = 0;
            int count = 0;
            for (var t = 0; t < seqLen; t++)
            {
                if (attentionMask[0, t] == 1)
                {
                    sum += lastHiddenState[0, t, h];
                    count++;
                }
            }
            pooled[h] = count > 0 ? sum / count : 0;
        }

        return pooled;
    }

    private static float[] L2Normalize(float[] vector)
    {
        var norm = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (norm < 1e-12f) return vector;
        return vector.Select(v => v / norm).ToArray();
    }
}
