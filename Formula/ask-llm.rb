class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.3.1"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.1/askllm-v1.3.1-osx-arm64"
      sha256 "058FE3D13824B4C691037D6B187805C52C9BBA0B07FBA983AD5F286DF17F5900"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.1/askllm-v1.3.1-osx-x64"
      sha256 "F6452991009297D657CD899C75A775EEC0F8276099716AF1123ED44C2E665557"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.1/askllm-v1.3.1-linux-x64"
    sha256 "7F4774E4DB353C84031BD9280C2C19D5AE559C9D4D69D2555958E34D301039E0"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.3.1-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.3.1-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.3.1-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end


