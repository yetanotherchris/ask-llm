class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.1.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.1.0/askllm-v1.1.0-osx-arm64"
      sha256 "PLACEHOLDER_ARM64_HASH"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.1.0/askllm-v1.1.0-osx-x64"
      sha256 "PLACEHOLDER_X64_HASH"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.1.0/askllm-v1.1.0-linux-x64"
    sha256 "PLACEHOLDER_LINUX_HASH"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.1.0-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.1.0-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.1.0-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end
