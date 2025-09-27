class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.4.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.4.0/askllm-v1.4.0-osx-arm64"
      sha256 "6a0d5e9bc72cda84344af08bcc661ec47f38d2f744afadab42a11d3631aea514"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.4.0/askllm-v1.4.0-osx-x64"
      sha256 "7ea138a0e27e54b10822df9698848b1e472f974767b94849dde045b6ec89c836"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.4.0/askllm-v1.4.0-linux-x64"
    sha256 "e1f5c49aaa533063c234abd48531e083c17e84d9d2734588310a6859e9e64fb3"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.4.0-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.4.0-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.4.0-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end




